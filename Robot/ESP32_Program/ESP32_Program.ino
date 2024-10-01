#include "esp_camera.h"
#include "FS.h"
#include "SD_MMC.h"
#include <Preferences.h>
#include <WiFi.h>

#define PWDN_GPIO_NUM     32
#define RESET_GPIO_NUM    -1
#define XCLK_GPIO_NUM      0
#define SIOD_GPIO_NUM     26
#define SIOC_GPIO_NUM     27

#define Y9_GPIO_NUM       35
#define Y8_GPIO_NUM       34
#define Y7_GPIO_NUM       39
#define Y6_GPIO_NUM       36
#define Y5_GPIO_NUM       21
#define Y4_GPIO_NUM       19
#define Y3_GPIO_NUM       18
#define Y2_GPIO_NUM        5
#define VSYNC_GPIO_NUM    25
#define HREF_GPIO_NUM     23
#define PCLK_GPIO_NUM     22

const char* ssid = "NETWORKIDHERE";
const char* password = "NETWORKPASSWDHERE";

bool byteSaving;

unsigned short cameraStreamInterval;
unsigned long cameraStreamBaseMillis;
bool cameraEnable;
byte cameraQualityPreset;
bool SDCardMounted;

unsigned short RRSIUpdateInterval;
unsigned long RRSIUpdateBaseMillis;

unsigned short NetworkResponseInterval;
unsigned long NetworkResponseBaseMillis;

Preferences preferences;
camera_config_t config;

void PrepareSDCard()
{
  if(SD_MMC.begin())
  {
    SDCardMounted = true;
    SD_MMC.mkdir("/DCIM");
    SD_MMC.mkdir("/DCIM/FRAMESIZE_XGA");
    SD_MMC.mkdir("/DCIM/FRAMESIZE_VGA");
    SD_MMC.mkdir("/DCIM/FRAMESIZE_QVGA");
  }
  else
  {
    preferences.begin("EEPROM-CAMERA", false);
    unsigned int counter;
    
    counter = preferences.getUInt("counterXGA", 0);
    if (counter != 0) counter = 0;
    preferences.putUInt("counterXGA", counter);

    counter = preferences.getUInt("counterVGA", 0);
    if (counter != 0) counter = 0;
    preferences.putUInt("counterVGA", counter);

    counter = preferences.getUInt("counterQVGA", 0);
    if (counter != 0) counter = 0;
    preferences.putUInt("counterQVGA", counter);
  }
}

void ImageSavePrepare()
{
  if(cameraQualityPreset == 0)
  {
    ImageSaveOnSD("FRAMESIZE_XGA", "counterXGA");
  }
  else if(cameraQualityPreset == 1)
  {
    ImageSaveOnSD("FRAMESIZE_VGA", "counterVGA");
  }
  else if(cameraQualityPreset == 2)
  {
    ImageSaveOnSD("FRAMESIZE_QVGA", "counterQVGA");
  }
}

void ImageSaveOnSD(String dirName, String counterName)
{
  if(SDCardMounted)
  {
    preferences.begin("EEPROM-CAMERA", false);
    
    unsigned int counter = preferences.getUInt(counterName.c_str(), 0);
    String path = "/DCIM/" + dirName + "/IMAGE_" + String(counter) + ".jpg";
    //String path = "/IMAGE_" + String(counter) + ".jpg";
      
    fs::FS &fs = SD_MMC;
    File file = fs.open(path.c_str(), FILE_WRITE);
    if(!file)
    {
      //Serial.println("Failed to open file in writing mode");
    }

    camera_fb_t * fb = NULL;
    esp_err_t res = ESP_OK;
    fb = esp_camera_fb_get();
    size_t bufferLength = fb->len;
    
    file.write(fb->buf, bufferLength);
    file.close();
    esp_camera_fb_return(fb);

    counter++;
    preferences.putUInt(counterName.c_str(), counter);
      
    preferences.end();

    const byte readyToMoveTrue[8] = {0x23, 0x53, 0x57, 0x00, 0xFF, 0x5F, 0xFF, 0x23};
    Serial.write(readyToMoveTrue, 8);
  }
}

void ImageRequestCapture(WiFiClient client, bool quality)
{
  const byte cameraConfirmedSDLocal[8] = {0x23, 0x43, 0x5F, 0x00, 0x00, 0xFF, 0x00, 0x23};
  const byte cameraConfirmedHDLocal[8] = {0x23, 0x43, 0x5F, 0x00, 0x00, 0xFF, 0xFF, 0x23};
  byte previousPreset = cameraQualityPreset;

  if(quality)
  {
    ApplyNewConfig(1);
    ImageStreamSend(client);
    client.write(cameraConfirmedSDLocal, 8);
  }
  else
  {
    ApplyNewConfig(0);
    ImageStreamSend(client);
    client.write(cameraConfirmedHDLocal, 8);
  }
  
  ApplyNewConfig(previousPreset);
}

void ImageStreamSend(WiFiClient client)
{
  camera_fb_t * fb = NULL;
  esp_err_t res = ESP_OK;
  fb = esp_camera_fb_get();

  if(!fb) {
    esp_camera_fb_return(fb);
    return;
  }

  if(fb->format != PIXFORMAT_JPEG)  {
    return;
  }
  
  size_t bufferLength = fb->len;
  byte byteArray[8];

  //A protocol message about expected size of the image
  byteArray[0] = 0x23;
  byteArray[1] = 0x4A;
  byteArray[2] = 0x5F;
  byteArray[3] = bufferLength & 0x000000ff;
  byteArray[4] = (bufferLength & 0x0000ff00) >> 8;
  byteArray[5] = (bufferLength & 0x00ff0000) >> 16;
  byteArray[6] = (bufferLength & 0xff000000) >> 24;
  byteArray[7] = 0x23;

  client.write(byteArray, 8);
  client.write(fb->buf, bufferLength);
  
  esp_camera_fb_return(fb);
}

WiFiServer server(60555);

void setup()
{
  Serial.begin(115200);
  
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sscb_sda = SIOD_GPIO_NUM;
  config.pin_sscb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG;
  
  config.frame_size = FRAMESIZE_VGA;
  config.jpeg_quality = 12;
  config.fb_count = 1;

  esp_err_t err_init = esp_camera_init(&config);
  if (err_init != ESP_OK)
  {
    delay(1000);
    ESP.restart();
  }

  SDCardMounted = false;
  PrepareSDCard();
  
  delay(50);
  WiFi.begin(ssid, password);
  
  while (WiFi.status() != WL_CONNECTED) 
  {
    delay(100);
  }
  
  server.begin();

  cameraQualityPreset = 1;
  cameraEnable = false;
  cameraStreamInterval = 100;
  cameraStreamBaseMillis = millis();

  RRSIUpdateInterval = 3000;
  RRSIUpdateBaseMillis = millis();

  NetworkResponseInterval = 10;
  NetworkResponseBaseMillis = millis();
}

void loop() 
{
  WiFiClient client = server.available();

  if(client) 
  {
    FlushSocketBuffer(client);
    SendWiFiReadyMessage();
    
    while(client.connected())
    {
      if((millis() - cameraStreamBaseMillis >= cameraStreamInterval) and cameraEnable)
      {
        ImageStreamSend(client);
        cameraStreamBaseMillis = millis();
      }

      if(millis() - RRSIUpdateBaseMillis >= RRSIUpdateInterval)
      {
        long RRSIValue = WiFi.RSSI() * (-1);
        unsigned short RRSIValueUShort = (unsigned short) RRSIValue;
        
        byte byteArrayRSSI[8];
        byteArrayRSSI[0] = 0x23;
        byteArrayRSSI[1] = 0x44;
        byteArrayRSSI[2] = 0x5F;
        byteArrayRSSI[3] = 0x00;
        byteArrayRSSI[4] = 0xFF;
        byteArrayRSSI[5] = RRSIValueUShort & 0x000000ff;
        byteArrayRSSI[6] = (RRSIValueUShort & 0x0000ff00) >> 8;
        byteArrayRSSI[7] = 0x23;

        client.write(byteArrayRSSI, 8);
        
        RRSIUpdateBaseMillis = millis();
      }

      if(millis() - NetworkResponseBaseMillis >= NetworkResponseInterval)
      {
        if(Serial.available() >= 8)
        {
          DataCheckProcess(client);
        }
      
        if(client.available() >= 8)
        {
          byte WiFiDataBytes[8] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
          
          for(byte i = 0; i < 8; i++)
          {
            WiFiDataBytes[i] = client.read();
          }
          ProcessIncomingWiFiData(WiFiDataBytes, client);
        }
        
        NetworkResponseBaseMillis = millis();
      }
    }
    
    SendWiFiDownMessage();
    cameraEnable = false;
  }

  if(millis() - NetworkResponseBaseMillis >= NetworkResponseInterval)
  {
    if(Serial.available() >= 8)
    {
      DataCheckProcess(client);
    }
    NetworkResponseBaseMillis = millis();
  }
}

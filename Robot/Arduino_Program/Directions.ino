void MoveForward()
{
  digitalWrite(MOTOR_IN1, HIGH);
  digitalWrite(MOTOR_IN2, LOW);
  digitalWrite(MOTOR_IN3, HIGH);
  digitalWrite(MOTOR_IN4, LOW);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, LOW);
}

void Stop()
{
  digitalWrite(MOTOR_IN1, LOW);
  digitalWrite(MOTOR_IN2, LOW);
  digitalWrite(MOTOR_IN3, LOW);
  digitalWrite(MOTOR_IN4, LOW);
  analogWrite(MOTOR_PWM, 0);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, LOW);
}

void MoveLeft()
{
  digitalWrite(MOTOR_IN1, HIGH);
  digitalWrite(MOTOR_IN2, LOW);
  digitalWrite(MOTOR_IN3, HIGH);
  digitalWrite(MOTOR_IN4, LOW);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, HIGH);
  digitalWrite(MOTOR_RELAY2, LOW);
}

void MoveRight()
{
  digitalWrite(MOTOR_IN1, HIGH);
  digitalWrite(MOTOR_IN2, LOW);
  digitalWrite(MOTOR_IN3, HIGH);
  digitalWrite(MOTOR_IN4, LOW);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, HIGH);
}

void MoveReverse()
{
  digitalWrite(MOTOR_IN1, LOW);
  digitalWrite(MOTOR_IN2, HIGH);
  digitalWrite(MOTOR_IN3, LOW);
  digitalWrite(MOTOR_IN4, HIGH);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, LOW);
}

void MoveReverseLeft()
{
  digitalWrite(MOTOR_IN1, LOW);
  digitalWrite(MOTOR_IN2, HIGH);
  digitalWrite(MOTOR_IN3, LOW);
  digitalWrite(MOTOR_IN4, HIGH);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, HIGH);
  digitalWrite(MOTOR_RELAY2, LOW);
}

void MoveReverseRight()
{
  digitalWrite(MOTOR_IN1, LOW);
  digitalWrite(MOTOR_IN2, HIGH);
  digitalWrite(MOTOR_IN3, LOW);
  digitalWrite(MOTOR_IN4, HIGH);
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, HIGH);
}

void RelaysOpen()
{
  analogWrite(MOTOR_PWM, 0);
  digitalWrite(MOTOR_RELAY1, HIGH);
  digitalWrite(MOTOR_RELAY2, HIGH);
}

void RelaysClosed()
{
  analogWrite(MOTOR_PWM, PWM_DutyCycle);
  digitalWrite(MOTOR_RELAY1, LOW);
  digitalWrite(MOTOR_RELAY2, LOW);
}

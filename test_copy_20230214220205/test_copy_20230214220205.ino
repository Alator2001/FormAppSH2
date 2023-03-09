

// void setup()
// {
//   Serial.begin(9600);
//   pinMode(PA6, OUTPUT);

//   Timer3.pause();
//   Timer3.setPrescaleFactor(7200); //один тик равен 100 микросекундам
//   Timer3.setInputCaptureMode(TIMER_CH1, TIMER_IC_INPUT_DEFAULT); 
//   Timer3.setInputCaptureMode(TIMER_CH2, TIMER_IC_INPUT_SWITCH); 
//   Timer3.setPolarity(TIMER_CH2, 1); 
//   Timer3.setSlaveFlags(TIMER_SMCR_TS_TI1FP1 | TIMER_SMCR_SMS_RESET);
//   Timer3.refresh();
//   Timer3.resume(); 
// }

// void loop()
// {
//   if(Timer3.getInputCaptureFlag(TIMER_CH1)) 
//   {
//     unsigned long period = Timer3.getCompare(TIMER_CH1);
//     unsigned long power = period * k;
//     Serial.print(period);
//     Serial.print("\t");
//     Serial.println(power);
//   }
//   digitalWrite(PA6, HIGH); 
//   int dl1 = random(50, 200);
//   delay(dl1); 
//   digitalWrite(PA6, LOW);
//   delay(dl1);
// }
//..........................................................................................
void setup()
{
Serial.begin(9600);
pinMode(PA6, INPUT);

Timer3.pause();
Timer3.setPrescaleFactor(7200); //один тик равен 100 микросекундам
Timer3.setInputCaptureMode(TIMER_CH1, TIMER_IC_INPUT_DEFAULT);
Timer3.setInputCaptureMode(TIMER_CH2, TIMER_IC_INPUT_SWITCH);
Timer3.setPolarity(TIMER_CH2, 1);
Timer3.setSlaveFlags(TIMER_SMCR_TS_TI1FP1 | TIMER_SMCR_SMS_RESET);
Timer3.refresh();
Timer3.resume();
}

void loop()
{
  if(Timer3.getInputCaptureFlag(TIMER_CH1))
  {    
    uint32_t period = Timer3.getCompare(TIMER_CH1);
    if (period != 0)
    {      
      //unsigned double power = (60/(period * 0.0001));
      Serial.println(period);
      //Serial.print("\t");
      //Serial.println(power);

    }

      
  }
}


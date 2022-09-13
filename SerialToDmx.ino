
#include <DmxSimple.h>

void setup() {
  Serial.begin(115200);

  pinMode(2, OUTPUT);
  digitalWrite(2, HIGH);
  DmxSimple.usePin(4); 

  setZero();
  Serial.println("SerialToDmx ready");
  Serial.println();
  Serial.println("Syntax:");
  Serial.println(" 123c : use DMX channel 123");
  Serial.println(" 45w  : set current channel to value 45");
}

int value = 0;
int channel;

void loop() {
  int c;

  while(!Serial.available());
  c = Serial.read();
  if ((c>='0') && (c<='9')) {
    value = 10*value + c - '0';
  } else {
    if (c=='c') channel = value;
    else if (c=='w') {
      DmxSimple.write(channel, value);
      Serial.print("Ch:");
      Serial.print(channel);
      Serial.print(" Value:");
      Serial.print(value);
      Serial.println();
    }
    value = 0;
  }
  if(c=='z'){
    setZero();
  }

}


void setZero(){
  
  for(int i = 0; i<=255; i++){
    DmxSimple.write(i, 0);
  }
  Serial.println("All CH set to ZERO");
}


void mhx_move(int ch1, int p_deg, int t_deg){

  int p = map(p_deg, 0, 540, 0, 255); 
  int t = map(t_deg, 0, 270, 0, 255);
  DmxSimple.write(ch1, p);
  DmxSimple.write(ch1 + 1, t);
  
}

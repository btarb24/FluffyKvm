#include <USBHost_t36.h>
#include <Keyboard.h>
#include <Mouse.h>

USBHost myusb;
USBSerial serialHost(myusb);  // Prolific USB-Serial adapter

String _inputBuffer;
boolean _isActive;
boolean _jigglerEnabled;
unsigned long _lastJiggle;
const unsigned long _jiggleInterval = 4UL * 60UL * 1000UL; // 4 minutes in milliseconds

//////////////////////////////////////////////////
// Windows VK codes (all keys)
//////////////////////////////////////////////////
// Control keys
#define VK_BACK       0x08
#define VK_TAB        0x09
#define VK_RETURN     0x0D
#define VK_SHIFT      0x10
#define VK_CONTROL    0x11
#define VK_MENU       0x12 // Alt
#define VK_PAUSE      0x13
#define VK_CAPITAL    0x14 // CapsLock
#define VK_ESCAPE     0x1B
#define VK_SPACE      0x20
#define VK_PRIOR      0x21 // PageUp
#define VK_NEXT       0x22 // PageDown
#define VK_END        0x23
#define VK_HOME       0x24
#define VK_LEFT       0x25
#define VK_UP         0x26
#define VK_RIGHT      0x27
#define VK_DOWN       0x28
#define VK_SNAPSHOT   0x2C // PrintScreen
#define VK_INSERT     0x2D
#define VK_DELETE     0x2E

// Numbers
#define VK_0          0x30
#define VK_1          0x31
#define VK_2          0x32
#define VK_3          0x33
#define VK_4          0x34
#define VK_5          0x35
#define VK_6          0x36
#define VK_7          0x37
#define VK_8          0x38
#define VK_9          0x39

// Letters
#define VK_A          0x41
#define VK_B          0x42
#define VK_C          0x43
#define VK_D          0x44
#define VK_E          0x45
#define VK_F          0x46
#define VK_G          0x47
#define VK_H          0x48
#define VK_I          0x49
#define VK_J          0x4A
#define VK_K          0x4B
#define VK_L          0x4C
#define VK_M          0x4D
#define VK_N          0x4E
#define VK_O          0x4F
#define VK_P          0x50
#define VK_Q          0x51
#define VK_R          0x52
#define VK_S          0x53
#define VK_T          0x54
#define VK_U          0x55
#define VK_V          0x56
#define VK_W          0x57
#define VK_X          0x58
#define VK_Y          0x59
#define VK_Z          0x5A

// Numpad
#define VK_NUMPAD0    0x60
#define VK_NUMPAD1    0x61
#define VK_NUMPAD2    0x62
#define VK_NUMPAD3    0x63
#define VK_NUMPAD4    0x64
#define VK_NUMPAD5    0x65
#define VK_NUMPAD6    0x66
#define VK_NUMPAD7    0x67
#define VK_NUMPAD8    0x68
#define VK_NUMPAD9    0x69
#define VK_MULTIPLY   0x6A
#define VK_ADD        0x6B
#define VK_SEPARATOR  0x6C
#define VK_SUBTRACT   0x6D
#define VK_DECIMAL    0x6E
#define VK_DIVIDE     0x6F

// Function keys
#define VK_F1         0x70
#define VK_F2         0x71
#define VK_F3         0x72
#define VK_F4         0x73
#define VK_F5         0x74
#define VK_F6         0x75
#define VK_F7         0x76
#define VK_F8         0x77
#define VK_F9         0x78
#define VK_F10        0x79
#define VK_F11        0x7A
#define VK_F12        0x7B

// Modifier keys
#define VK_SHIFT_LEFT   0xA0
#define VK_SHIFT_RIGHT  0xA1
#define VK_CONTROL_LEFT 0xA2
#define VK_ALT_LEFT     0xA4
#define VK_ALT_RIGHT    0xA5
#define VK_WIN          0x5B

// Multimedia keys
#define VK_VOLUME_MUTE    0xAD
#define VK_VOLUME_DOWN    0xAE
#define VK_VOLUME_UP      0xAF
#define VK_MEDIA_NEXT     0xB0
#define VK_MEDIA_PREV     0xB1
#define VK_MEDIA_STOP     0xB2
#define VK_MEDIA_PLAY_PAUSE 0xB3
#define VK_LAUNCH_MAIL    0xB4
#define VK_LAUNCH_MEDIA   0xB5

// Browser keys
#define VK_BROWSER_BACK      0xA6
#define VK_BROWSER_FORWARD   0xA7
#define VK_BROWSER_REFRESH   0xA8
#define VK_BROWSER_STOP      0xA9
#define VK_BROWSER_SEARCH    0xAA
#define VK_BROWSER_FAVORITES 0xAB
#define VK_BROWSER_HOME      0xAC

// OEM keys
#define VK_OEM_1 0xBA //semicolon/colon
#define VK_OEM_PLUS 0xBB //equals/plus
#define VK_OEM_COMMA 0xBC //comma/bracketL
#define VK_OEM_MINUS 0xBD //hyphen/underscore
#define VK_OEM_PERIOD 0xBE //period/bracketR
#define VK_OEM_2 0xBF //slashF/questionMark
#define VK_OEM_3 0xC0 //tilde/backtick
#define VK_OEM_4 0xDB //braceL
#define VK_OEM_5 0xDC //pipe/slashB
#define VK_OEM_6 0xDD //braceR
#define VK_OEM_7 0xDE //quote


//////////////////////////////////////////////////
// VK → HID mapping
//////////////////////////////////////////////////
int vkToHid(int vk) {
  switch (vk) {
    // Letters
    case VK_A: return KEY_A; case VK_B: return KEY_B; case VK_C: return KEY_C;
    case VK_D: return KEY_D; case VK_E: return KEY_E; case VK_F: return KEY_F;
    case VK_G: return KEY_G; case VK_H: return KEY_H; case VK_I: return KEY_I;
    case VK_J: return KEY_J; case VK_K: return KEY_K; case VK_L: return KEY_L;
    case VK_M: return KEY_M; case VK_N: return KEY_N; case VK_O: return KEY_O;
    case VK_P: return KEY_P; case VK_Q: return KEY_Q; case VK_R: return KEY_R;
    case VK_S: return KEY_S; case VK_T: return KEY_T; case VK_U: return KEY_U;
    case VK_V: return KEY_V; case VK_W: return KEY_W; case VK_X: return KEY_X;
    case VK_Y: return KEY_Y; case VK_Z: return KEY_Z;

    // Numbers
    case VK_0: return KEY_0; case VK_1: return KEY_1; case VK_2: return KEY_2;
    case VK_3: return KEY_3; case VK_4: return KEY_4; case VK_5: return KEY_5;
    case VK_6: return KEY_6; case VK_7: return KEY_7; case VK_8: return KEY_8;
    case VK_9: return KEY_9;

    // Numpad
    case VK_NUMPAD0: return KEYPAD_0;
    case VK_NUMPAD1: return KEYPAD_1;
    case VK_NUMPAD2: return KEYPAD_2;
    case VK_NUMPAD3: return KEYPAD_3;
    case VK_NUMPAD4: return KEYPAD_4;
    case VK_NUMPAD5: return KEYPAD_5;
    case VK_NUMPAD6: return KEYPAD_6;
    case VK_NUMPAD7: return KEYPAD_7;
    case VK_NUMPAD8: return KEYPAD_8;
    case VK_NUMPAD9: return KEYPAD_9;
    case VK_DECIMAL: return KEYPAD_PERIOD;
    case VK_ADD:     return KEYPAD_PLUS;
    case VK_SUBTRACT:return KEYPAD_MINUS;
    case VK_MULTIPLY:return KEYPAD_ASTERIX;
    case VK_DIVIDE:  return KEYPAD_SLASH;

    // Controls
    case VK_RETURN:   return KEY_ENTER;
    case VK_SPACE:    return KEY_SPACE;
    case VK_BACK:     return KEY_BACKSPACE;
    case VK_TAB:      return KEY_TAB;
    case VK_ESCAPE:   return KEY_ESC;
    case VK_DELETE:   return KEY_DELETE;
    case VK_INSERT:   return KEY_INSERT;
    case VK_HOME:     return KEY_HOME;
    case VK_END:      return KEY_END;
    case VK_PRIOR:    return KEY_PAGE_UP;
    case VK_NEXT:     return KEY_PAGE_DOWN;
    case VK_SNAPSHOT: return KEY_PRINTSCREEN;

    // Arrows
    case VK_LEFT: return KEY_LEFT;
    case VK_RIGHT:return KEY_RIGHT;
    case VK_UP:   return KEY_UP;
    case VK_DOWN: return KEY_DOWN;

    // Function keys
    case VK_F1: return KEY_F1; case VK_F2: return KEY_F2;
    case VK_F3: return KEY_F3; case VK_F4: return KEY_F4;
    case VK_F5: return KEY_F5; case VK_F6: return KEY_F6;
    case VK_F7: return KEY_F7; case VK_F8: return KEY_F8;
    case VK_F9: return KEY_F9; case VK_F10: return KEY_F10;
    case VK_F11: return KEY_F11; case VK_F12: return KEY_F12;

    // OEM keys
    case VK_OEM_1: return KEY_SEMICOLON; case VK_OEM_PLUS: return KEY_EQUAL;
    case VK_OEM_COMMA: return KEY_COMMA; case VK_OEM_MINUS: return KEY_MINUS;
    case VK_OEM_PERIOD: return KEY_PERIOD; case VK_OEM_2: return KEY_SLASH;
    case VK_OEM_3: return KEY_TILDE; case VK_OEM_4: return KEY_LEFT_BRACE;
    case VK_OEM_5: return KEY_BACKSLASH; case VK_OEM_6: return KEY_RIGHT_BRACE;
    case VK_OEM_7: return KEY_QUOTE;

    // Modifiers
    case VK_SHIFT:        return KEY_LEFT_SHIFT;
    case VK_CONTROL:      return KEY_LEFT_CTRL;
    case VK_MENU:         return KEY_LEFT_ALT;
    case VK_CAPITAL:      return KEY_CAPS_LOCK;
    case VK_SHIFT_LEFT:   return KEY_LEFT_SHIFT;
    case VK_SHIFT_RIGHT:  return KEY_RIGHT_SHIFT;
    case VK_CONTROL_LEFT: return KEY_LEFT_CTRL;
    case VK_ALT_LEFT:     return KEY_LEFT_ALT;
    case VK_ALT_RIGHT:    return KEY_RIGHT_ALT;
    case VK_WIN:          return KEY_LEFT_GUI;

    // Multimedia keys (Consumer HID)
    case VK_VOLUME_MUTE:      return KEY_MEDIA_MUTE;
    case VK_VOLUME_DOWN:      return KEY_MEDIA_VOLUME_DEC;
    case VK_VOLUME_UP:        return KEY_MEDIA_VOLUME_INC;
    case VK_MEDIA_NEXT:       return KEY_MEDIA_NEXT_TRACK;
    case VK_MEDIA_PREV:       return KEY_MEDIA_PREV_TRACK;
    case VK_MEDIA_STOP:       return KEY_MEDIA_STOP;
    case VK_MEDIA_PLAY_PAUSE: return KEY_MEDIA_PLAY_PAUSE;

    default: return 0;
  }
}

//////////////////////////////////////////////////
// Teensy Relay Logic
//////////////////////////////////////////////////
void setup() {
  Serial.begin(115200); // Debug
  Serial1.begin(115200); // FTDI
  myusb.begin();
  Keyboard.begin();
  Mouse.begin();

  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(10, OUTPUT); //red
  pinMode(11, OUTPUT); //green
  pinMode(12, OUTPUT); //yellow
  pinMode(5, INPUT_PULLUP); //switch
  
  attachInterrupt(digitalPinToInterrupt(5), evaluateJigglerSwitch, CHANGE);

  evaluateJigglerSwitch();
  setInactive();
}

void loop() {
  myusb.Task();
  
  unsigned long now = millis();

  while (Serial1.available() > 0) {
    digitalWrite(LED_BUILTIN, HIGH);  // Turn on LED when processing a message
    char c = Serial1.read();
    if (c == '\n') {
      setActive();
      processMessage(_inputBuffer);
      _lastJiggle = now;
      _inputBuffer = "";
    } else {
      _inputBuffer += c;
    }
  }

  if (_jigglerEnabled && now - _lastJiggle >= _jiggleInterval){
    _lastJiggle = now;
    jiggle();
  }

  digitalWrite(LED_BUILTIN, LOW);
}

void evaluateJigglerSwitch(){
  if (digitalRead(5) == LOW)
    enableJiggler();
  else
    disableJiggler();
}

void enableJiggler(){
  _jigglerEnabled = true;
  digitalWrite(12, HIGH); 
  Serial.println("jiggler enabled");
}

void disableJiggler(){
  _jigglerEnabled = false;
  digitalWrite(12, LOW); 
  Serial.println("jiggler disabled");
}

void jiggle(){
  Serial.println("jiggle");

  int dx = random(-1, 2); // -1, 0, 1
  int dy = random(-1, 2); // -1, 0, 1

  if (dx == 0 && dy == 0) {
    dx = 1; // ensure at least some movement
  }

  Mouse.move(dx, dy, 0);
}

// Helper to ensure the target lock key matches the desired state
void setLockKey(bool desired, uint8_t key) {
  // Press and release the key once to toggle
  Keyboard.press(key);
  Keyboard.release(key);

  // If desired state is OFF, press/release again to turn it off
  if (!desired) {
    Keyboard.press(key);
    Keyboard.release(key);
  }
}

void processMessage(String msg) {
  Serial.println(msg);
  int firstSep = msg.indexOf('_');
  if (firstSep == -1)
   return;

  int type = msg.substring(0, firstSep).toInt();
  String data = msg.substring(firstSep + 1);

  switch (type) {
    case 0: { // KeyDown
      int vk = data.toInt();
      int code = vkToHid(vk);
      if (code) {
        Keyboard.press(code);
      }
      break;
    }

    case 1: { // KeyUp
      int vk = data.toInt();
      int code = vkToHid(vk);
      if (code) {
        Keyboard.release(code);
      }
      break;
    }

    case 2: { // MouseMove dx_dy
      int sep = data.indexOf('_');
      if (sep == -1)
        return;

      int dx = data.substring(0, sep).toInt();
      int dy = data.substring(sep + 1).toInt();
      Mouse.move(dx, dy);
      break;
    }

    case 3: { // MouseDown
      long buttonFlags = data.toInt(); // Use long to accommodate large enum values

      if (buttonFlags & 0x100000) Mouse.press(MOUSE_LEFT);
      if (buttonFlags & 0x200000) Mouse.press(MOUSE_RIGHT);
      if (buttonFlags & 0x400000) Mouse.press(MOUSE_MIDDLE);
      if (buttonFlags & 0x800000) Mouse.press(MOUSE_BACK);
      if (buttonFlags & 0x1000000) Mouse.press(MOUSE_FORWARD);

      break;
    }

    case 4: { // MouseUp
      long buttonFlags = data.toInt(); // Use long to accommodate large enum values

      if (buttonFlags & 0x100000) Mouse.release(MOUSE_LEFT);
      if (buttonFlags & 0x200000) Mouse.release(MOUSE_RIGHT);
      if (buttonFlags & 0x400000) Mouse.release(MOUSE_MIDDLE);
      if (buttonFlags & 0x800000) Mouse.release(MOUSE_BACK);
      if (buttonFlags & 0x1000000) Mouse.release(MOUSE_FORWARD);

      break;
    }

    case 5: { // MouseWheel
      int delta = data.toInt();
      Mouse.scroll(delta / 120);
      break;
    }

    case 6: { // LockKeySync
      int state = data.toInt();
      setLockKey(state & 1, KEY_CAPS_LOCK);   // CapsLock
      setLockKey(state & 2, KEY_NUM_LOCK);    // NumLock
      setLockKey(state & 4, KEY_SCROLL_LOCK); // ScrollLock
      break;
    }

    case 7: { // active state
      int active = data.toInt();
      if (active == 0)
        setInactive();
      else
        setActive();

      break;
    }
  }
}

void setActive(){
  if (_isActive == true)
    return;

  Serial.println("Server active");
  _isActive = true;
  digitalWrite(10, LOW); 
  digitalWrite(11, HIGH); 
}

void setInactive(){
  Serial.println("Server inactive");
  _isActive = false;
  digitalWrite(10, HIGH);
  digitalWrite(11, LOW); 
}
Schreibe mir eine .NET 10 Desktop-Anwendung mit Avalonia UI zur Ansteuerung eines INA228 Strom-/Spannungssensors über I2C mittels eines FTDI UM232H Chips.

Anforderungen:
1. **Hardware-Kommunikation**: I2C-Zugriff auf den INA228 (Spannung, Strom, Power) über den UM232H (z.B. via libmpsse / SharpCompress / FTDI D2XX oder Microsoft.Device.I2C / Iot.Device.Bindings, falls kompatibel).
2. **Prozessdaten & Abtastung**: Ein asynchroner background task, der stabile Abtastraten von 20ms bis 50ms (konfigurierbar) ohne UI-Blockaden realisiert.
3. **Datenlogger**: Zyklisches Speichern der Messwerte (Zeitstempel, Spannung, Strom, Leistung) in eine CSV-Datei im Hintergrund.
4. **Avalonia UI**:
   - Ein modernes Dashboard.
   - Echtzeit-Diagramme (Charts) für Spannung, Strom und Power (nutze z.B. LiveChartsCore oder OxyPlot für Avalonia). 
   - Echtzeit Daten wie Temperatur aber mit niedrige Abtastzeit
   - Start-/Stopp-Button für den Logger und Regler für das Abtastintervall.
   - INA228 Einstellungen wie ADCRange, Conversion Zeit für VBUS, Shunt Voltage, Temperatur, sowie ADC Sample Averaging count

Bitte liefere die Architektur, die wichtigsten Services (I2C-Read, Logger) und das ViewModel/View-Setup für Avalonia.
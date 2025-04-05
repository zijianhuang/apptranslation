
Based on https://docs.oasis-open.org/xliff/v1.2/os/xliff-core.html

```
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" xliff-core-1.2-strict.xsd /classes /namespace:Xliff12Lib
```
Manually modify file into filex.

Remarks:
* While XLIFF supports interpolation, the XML schema v1.2 and v2.0 do not support well XML serialization without altering the semantic structures. Therefore, the XSD.exe generated codes for forward-only non-cache writter can not preserve the original order of text nodes and interpolation elements.


<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="/">
    <html>
      <head>
        <title>Language Resources</title>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <meta http-equiv="Cache-control" content="no-cache, no-store, must-revalidate" />
        <link rel="preload" as="style" onload="this.rel = 'stylesheet'" href="resx.css" />
      </head>
      <body>
        <table class="resx table-striped">
          <tr>
            <th>Key</th>
            <th>Value</th>
            <th>Comment</th>
          </tr>
          <xsl:for-each select="root/data">
            <tr>
              <td>
                <xsl:value-of select="@name"/>
              </td>
              <td>
                <xsl:value-of select="value"/>
              </td>
              <td>
                <xsl:value-of select="comment"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>        
      </body>      
    </html>

  </xsl:template>


</xsl:stylesheet>

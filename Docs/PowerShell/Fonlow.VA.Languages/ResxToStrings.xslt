<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="/">
    <resources>
      <xsl:for-each select="root/data">
        <string>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:if test="comment">
            <xsl:attribute name="comment">
              <xsl:value-of select="comment"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:value-of select="value"/>
        </string>
      </xsl:for-each>
    </resources>
  </xsl:template>


</xsl:stylesheet>

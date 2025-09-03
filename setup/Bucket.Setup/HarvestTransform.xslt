<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://wixtoolset.org/schemas/v4/wxs">

  <!-- Copy everything by default -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Remove PDB files (debug symbols) -->
  <xsl:template match="wix:Component[wix:File[contains(@Source, '.pdb')]]" />

  <!-- Remove XML documentation files -->
  <xsl:template match="wix:Component[wix:File[contains(@Source, '.xml') and not(contains(@Source, '.config'))]]" />

  <!-- Remove development JSON files -->
  <xsl:template match="wix:Component[wix:File[contains(@Source, '.dev.json')]]" />

  <!-- Remove temporary files -->
  <xsl:template match="wix:Component[wix:File[contains(@Source, '.tmp')]]" />

  <!-- Remove reference assemblies -->
  <xsl:template match="wix:Component[wix:File[contains(@Source, '\ref\')]]" />

</xsl:stylesheet>

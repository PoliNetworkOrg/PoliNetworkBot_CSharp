namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Data;

public class Const
{
    public const string HrefRepairLink = "https://www7.ceda.polimi.it/spazi/spazi/controller/";

    public const string PolimiController =
        "https://www7.ceda.polimi.it/spazi/spazi/controller/OccupazioniGiornoEsatto.do";

    public const string HtmlTableInit = @"
<div id=""tableContainer"" class=""tableContainer"">
	<table padding=""0"" spacing=""0"" class=""scrollTable"" style=""table-layout: fixed; width: 100%;"">				
			<tbody class=""scrollContent"">";

    public const string HtmlTableEnd = @"</tbody></table></div>";

    public const string HtmlClockLine = @"
<tr><td class=""innerDataDove"">Data</td><td class=""innerDataDove"">Dove</td><td class=""innerOrario"" colspan=""2""></td><td class=""innerOrario"" colspan=""4"">09:00</td><td class=""innerOrario"" colspan=""4"">10:00</td><td class=""innerOrario"" colspan=""4"">11:00</td><td class=""innerOrario"" colspan=""4"">12:00</td><td class=""innerOrario"" colspan=""4"">13:00</td><td class=""innerOrario"" colspan=""4"">14:00</td><td class=""innerOrario"" colspan=""4"">15:00</td><td class=""innerOrario"" colspan=""4"">16:00</td><td class=""innerOrario"" colspan=""4"">17:00</td><td class=""innerOrario"" colspan=""4"">18:00</td><td class=""innerOrario"" colspan=""4"">19:00</td><td class=""innerOrario"" colspan=""4"">20:00</td><td class=""innerOrario"" colspan=""2""></td></tr>
<tr><td colspan=""2"" class=""innerGiorno""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td><td style=""height:10"" class=""riferimento_orario_prima""></td><td class=""empty""></td><td class=""empty""></td><td class=""riferimento_orario_ultima""></td></tr>";

    public const string CssStyles = @"
<head>
<style>
body {
	background: #FFF;
	color: #000;
	margin: 0;
	padding: 0;
	border: #333 1px solid;
}



table.scrollTable{ 
	table-layout: fixed;
	border-collapse: collapse;
    border: #333 1px solid; 
    width: 98,5%;
	border: #333 1px solid;
}

table.scrollTable tbody tr{
	font-family: Verdana,Arial,sans-serif,serif;
	font-size: 0.8em;
	padding: 2px 2px 2px 2px
	border: #333 1px solid;
}

table.scrollTable td {
	font-family: Verdana,Arial,sans-serif,serif;
    font-weight: bold;
	font-size: 1.1em;
	padding: 2px 2px 2px 2px;	
	overflow: hidden;
	empty-cells: show;   
}

table.scrollTable tr:hover { 
	background-color:#F3F3F3;
}

table.scrollTable thead tr  {
	border: #333 1px solid;
	font-family: Verdana,Arial,sans-serif,serif;
	font-size: 0.6em;
	background-color: #aaaaaa;
}

table.scrollTable th {
	border: #333 0px solid;
	font-family: Arial,sans-serif,serif;
	font-size: 1em;
	background-color: #aaaaaa;
}

table.scrollTable tbody tr td.empty {
	padding: 2px 2px 2px 2px;	
}

.data{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;
	border: #333 1px solid;
    /*background: #C4E0E8;*/
    background: #F3F3EE;
    color: #000000;
}
.dove{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;
	border: #333 1px solid;
    /* background: #C4E0E8; */
    background: #F3F3EE;
    color: #000000;
	text-align: center;	
}

.empty{
	border: #333 1px solid;
}

.empty_prima{
	border: #333 1px solid;
    /* background: #dcdcdc; */
    background: #EFEEF1;
}

.riferimento_orario_prima{
	border: #333 1px solid;
    /* background: #dcdcdc; */
    background: #EFEEF1;
}
.riferimento_orario_ultima{
	border: #333 1px solid;
	border-right-width: 3px;	
}
.slot{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;	
	border: #333 1px solid;
    /* background: #C4E0E8; */
    /* background: #F3F3EE; */
    background:#cce6ff;
    color: #000000;
    height: auto !important;
    height: 40px;
    min-height: 40px;
}

.normalRow{
    height: auto !important;
    height: 40px;
    min-height: 40px;
}

.header{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;	
	border: #333 1px solid;
    background: #aaaaaa;
    color: #000000;
	text-align: center;	
}

.innerOrario{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;	
	border: #333 0px none;
	/* background: #dcdcdc; */
	background: #EFEEF1;
    color: #000000;
	text-align: center;	
}
.innerDataDove{
	font-size: 0.8em;
	padding: 2px 2px 2px 2px;	
	border: #333 1px solid;
	/* background: #dcdcdc; */
	background: EFEEF1;
    color: #000000;
	text-align: center;	
	width: 80px;
}
.innerEdificio{
	font-size: 1em;
	padding: 10px 5px;	
	border: #333 1px solid;
	border-top-width: 4px;
    /* background: #b8b8b8; */
    background: #ABB5BF;
    color: #000000;
	text-align: left;	
}
.innerGiorno{
	font-size: 1em;
	border: #333 1px solid;
    /* background: #dcdcdc; */
    background: #EFEEF1;
    color: #000000;
	text-align: left;	
}

</style>
</head>";
}
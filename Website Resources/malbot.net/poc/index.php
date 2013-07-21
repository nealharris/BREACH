
<!-- Copyright (c) 2006 Microsoft Corporation.  All rights reserved. -->
<!-- OwaPage = ASP.auth_error_aspx -->

<!-- {698798E9-889B-4145-ACFC-474C378C7B4F} -->
<html dir="ltr">



<head>
	<meta http-equiv="Content-Type" content="text/html; CHARSET=utf-8">
	<title>Error</title>
	<link type="text/css" rel="stylesheet" href="/owa/8.3.83.4/themes/base/premium.css">
	<link type="text/css" rel="stylesheet" href="/owa/8.3.83.4/themes/base/owafont.css">
	
	
</head>
<body class="err">

<table id="tblErr" cellspacing=0 cellpadding=0>
	<tr>
		<td>
			<table cellpadding=0 cellspacing=0>
				<tr>
					<td align="center" valign="middle"><img src="8.3.83.4/themes/base/logob.gif"></td>
					<td class="w100"></td>
					 
						<td nowrap id="tdErrLgf"><a href="logoff.owa?canary=bb63e4ba67e24dab81ed425c5a90d7c2">Log Off</a></td>

				</tr>
			</table>
		</td>
	</tr>
	<tr>
		<td>
			<table class="w100" cellpadding=0 cellspacing=0>
				<tr>
					<td><img src="8.3.83.4/themes/base/crvtplt.gif"></td>
					<td id="tdErrHdTp"><img src="8.3.83.4/themes/base/clear1x1.gif"></td>
					<td><img src="8.3.83.4/themes/base/crvtprt.gif"></td>
				</tr>
				<tr>
					<td colspan=3 id="tdErrHdCt">
						<div class="errHd" id="errMsg"><b>The item that you attempted to access appears to be corrupted and cannot be accessed.</b></div>
					
						<div class="errHd"><a href="?ae=Folder&t=IPF.Note&id=LgAAAACcgPE1EtvJQYneH2qPwZKgAQDrDy1FLCeMRoIhgTcxQwutAAABhE8vAAAB&pg=1&slUsng=0">Click here to continue working.</a></div>
					
					</td>
				</tr>
				<tr>
					<td colspan=3 class="errHk"><img src="8.3.83.4/themes/base/clear1x1.gif"></td>
				</tr>
			</table>
		</td>
	</tr>
	<tr>
		<td id="tdErrDbg">


	<div class=act onclick="document.getElementById('divDtls').style.display='';this.style.display='none';"><img src="/owa/8.3.83.4/themes/base/expnd.gif"> Show details</div>
	<br>
	<div id=divDtls style="display:none"><br><b>Request</b><br>Url: <span id=requestUrl>http://breachattack.com/poc/?<? echo str_replace('"', "&quot;", urldecode($_SERVER['QUERY_STRING'])); ?></span><br>User host address: <span id=userHostAddress>204.14.239.223</span><br>User: <span id=userName>Administrator</span><br>EX Address: <span id=exAddress>/o=Vozaa/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Recipients/cn=Administrator</span><br>SMTP Address: <span id=smtpAddress>Administrator@corp.vozaa.com</span><br>OWA version: <span id=owaVersion>8.3.83.4</span><br>Mailbox server: <span id=mailboxServer>VozaaVM-Medium.corp.vozaa.com</span><br><br><b>Exception</b><br>Exception type: <span id=exceptionType>Microsoft.Exchange.Data.Storage.CorruptDataException</span><br>Exception message: <span id=exceptionMessage>System.FormatException: Invalid length for a Base-64 char array.
   at System.Convert.FromBase64String(String s)
   at Microsoft.Exchange.Data.Storage.StoreId.Base64ToByteArray(String base64String)</span><br><br><b>Call stack</b><br><div id=exceptionCallStack><div nowrap>Microsoft.Exchange.Data.Storage.StoreId.Base64ToByteArray(String base64String)
  </div><div nowrap>Microsoft.Exchange.Data.Storage.StoreObjectId.Deserialize(String base64Id)
  </div><div nowrap>Microsoft.Exchange.Clients.Owa.Core.Utilities.CreateStoreObjectId(String storeObjectIdString)
  </div><div nowrap>Microsoft.Exchange.Clients.Owa.Basic.OwaForm.OnLoad(EventArgs e)
  </div><div nowrap>Microsoft.Exchange.Clients.Owa.Basic.EditMessage.OnLoad(EventArgs e)
  </div><div nowrap>System.Web.UI.Control.LoadRecursive()
  </div>System.Web.UI.Page.ProcessRequestMain(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)</div><br><b>Inner Exception</b><br>Exception type: <span id=exceptionType>System.FormatException</span><br>Exception message: <span id=exceptionMessage>Invalid length for a Base-64 char array.</span><br><br><b>Call stack</b><br><div id=exceptionCallStack><div nowrap>System.Convert.FromBase64String(String s)
  </div>Microsoft.Exchange.Data.Storage.StoreId.Base64ToByteArray(String base64String)</div></div>

		</td>
	</tr>
	<tr>
		<td>
			<table class="w100" cellpadding=0 cellspacing=0>
				<tr>
					<td colspan=3 class="errHk"><img src="8.3.83.4/themes/base/clear1x1.gif"></td>
				</tr>
				<tr>
					<td colspan=3 id="tdErrFtCt"><img src="8.3.83.4/themes/base/clear1x1.gif"></td>
				</tr>
				<tr>
					<td><img src="8.3.83.4/themes/base/crvbtmlt.gif"></td>
					<td id="tdErrFtBt"><img src="8.3.83.4/themes/base/clear1x1.gif"></td>
					<td><img src="8.3.83.4/themes/base/crvbtmrt.gif"></td>
				</tr>
			</table>
		</td>
	</tr>
</table>

</body>
</html>

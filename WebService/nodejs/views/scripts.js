
function getSeries(accession, dl)
{
	var xmlhttp = new XMLHttpRequest();
	xmlhttp.onreadystatechange = 
		function()
		{
			if (this.readyState == 4 && this.status == 200) 
			{
				var els = JSON.parse(this.responseText);
				updateDataList(els, dl);
			}
		};
		var pax = document.getElementById("sourcepax").value;
		xmlhttp.open("GET", "../seriesfor/"+pax+"/"+accession, true);
		xmlhttp.send();
}

function getAccessionsForPatient(accession)
{
	var xmlhttp = new XMLHttpRequest();
	xmlhttp.onreadystatechange = 
		function()
		{
			if (this.readyState == 4 && this.status == 200) 
			{
				var els = JSON.parse(this.responseText);
				var dataList = document.getElementById("accessionsDL");
				dataList.innerHTML = '';
				for (var i = 0; i < els.length; i++)
				{
					var opt = document.createElement('option');
					opt.value = els[i].number + ": " + els[i].description + " ["+formatDate(els[i].date)+"]"; 
					dataList.appendChild(opt);
				}
			}
		};
		var pax = document.getElementById("sourcepax").value;
		xmlhttp.open("GET", "../accessionsforpatient/"+pax+"/"+accession, true);
		xmlhttp.send();
}

function updateAccession(accession, dl)
{
	if (accession.lastIndexOf(":") != -1) accession = accession.substring(0, accession.lastIndexOf(":"));
	if (accession.length >= 14) 
	{
		getAccessionsForPatient(accession);
		getSeries(accession, dl);
	}
}

function updateDataList(elems, dl)
{
	var dataList = document.getElementById(dl);
	dataList.innerHTML = '';
	
	for (var i = 0; i < elems.length; i++)
	{
		var opt = document.createElement('option');
		opt.value = elems[i].description + " (" +elems[i].count+")";
		dataList.appendChild(opt);
	}
}

function formatDate(dstring)
{
	let year = dstring.substring(0, 4);
	let month = dstring.substring(4, 6);
	let day = dstring.substring(6, 8);
	
	if (month == "01") month = "-Jan-";
	else if (month == "02") month = "-Feb-";
	else if (month == "03") month = "-Mar-";
	else if (month == "04") month = "-Apr-";
	else if (month == "05") month = "-May-";
	else if (month == "06") month = "-Jun-";
	else if (month == "07") month = "-Jul-";
	else if (month == "08") month = "-Aug-";
	else if (month == "09") month = "-Sep-";
	else if (month == "10") month = "-Oct-";
	else if (month == "11") month = "-Nov-";
	else if (month == "12") month = "-Dec-";
	
	return day + month + year;
}

function init()
{
	let accession = document.getElementById("currentAccession").value;
	updateAccession(accession, "currentSeriesDL")	
}






const config = require('./config');

var express = require('express'),
	sql = require('mssql'),
	fs = require('fs'),
	http = require('http'),
	app = express();

app.set('view engine', 'pug');
app.use(express.static(__dirname + '/assets'));
app.set('views', __dirname + '/views');

app.get('/', function (req, res)
{
	console.log("Dashboard requested by " + req.connection.user + "[" + req.ip + "]");
	res.writeHead( 301, { "Location" : "/jobs/" });
	res.end();
});

app.get('/jobs/:lastid?', function (req, res) 
{
	console.log("Jobs requested by " + req.ip);
	
	var lastid = parseInt(req.params.lastid);

		sql.connect(config.SQL_CONFIG, function(err)
		{
			if (err) 
			{
				console.log(err);
				res.render('badDB');
				sql.close();
				return;
			}
			
			var request = new sql.Request();
			
			//var query = 'SELECT TOP(50) * from Jobs order by id desc';
            var query = 'SELECT TOP(50) * from Jobs INNER JOIN Attempts on Jobs.AttemptId = Attempts.Id INNER JOIN StoredRecipes on StoredRecipes.Id = Jobs.RecipeId order by Jobs.Id desc ';
			if (lastid) query = 'SELECT TOP(50) * from Jobs INNER JOIN Attempts on Jobs.AttemptId = Attempts.Id INNER JOIN StoredRecipes on StoredRecipes.Id = Jobs.RecipeId where Jobs.Id < '+  lastid +' order by Jobs.Id desc';
			
			
			request.query(query, function(err, recordset)
			{
				if (err) 
				{
					console.log(err);
					res.render('badDB');
					sql.close();
					return;
				}
				text = " where ";
				
				var jobs = []
				
				for (i =0; i < recordset.recordsets[0].length; ++i)
				{
					record = recordset.recordsets[0][i];
					
					var time;
					time = new Date(record.End - record.Start);
					time = time.getMinutes() + ":" + ('0' + time.getSeconds()).slice(-2);
				
					if (record.Status[0] == "Failed" || record.Status[0] == "Processing") time = "N/A";
					
					jobs.push({
						id: record.Id[0],
						name: record.PatientFullName,
						dob: record.PatientBirthDate,
						patientid: record.PatientId,
						current: record.CurrentAccession,
						prior: record.PriorAccession,
						source: record.SourceAet,
						dest: record.DestinationAet,
						status: record.Status[1],
						complete: record.End.toUTCString(),
						time: time,
					});
					
					lastid = record.Id;
				}

				res.render('jobs', {jobs : jobs, lastid: lastid});
				sql.close();
			});
		});
		

});

app.get('/attempts/:lastid?', function (req, res) 
{
	console.log("attempts requested by " + req.ip);
	
	var lastid = parseInt(req.params.lastid);

		sql.connect(config.SQL_CONFIG, function(err)
		{
			if (err) 
			{
				console.log(err);
				res.render('badDB');
				sql.close();
				return;
			}
			
			var request = new sql.Request();
			var query = 'SELECT TOP(50) * from Attempts order by id desc';
			if (lastid) query = 'SELECT TOP(50) * from Attempts where id < ' + lastid + ' order by id desc'
			
			request.query(query, function(err, recordset)
			{
				if (err) console.log(err);
				text = " where ";
				
				var attempts = []
				
				for (i =0; i < recordset.recordsets[0].length; ++i)
				{
					record = recordset.recordsets[0][i];
					
					attempts.push({
						id: record.Id,
						name: record.PatientFullName,
						dob: record.PatientBirthDate,
						patientid: record.PatientId,
						current: record.CurrentAccession,
						prior: record.PriorAccession,
						accession: record.Accession,
						status: record.Status,
						jobid: record.JobId,
						comment: record.Comment,
					});
					
					lastid = record.Id;
				}
			
				res.render('attempts', {vals : attempts, lastid: lastid});
				
				sql.close();
			});
		});
});

app.get('/settings/', function (req, res)
{
	console.log("Settings requested by " + req.ip);
	res.render('settings');
});

app.get('/log/', function (req, res)
{
	fs.readFile(config.LOG_PATH, {encoding: 'utf-8'}, function(err,data){
    if (!err) 
    {
          console.log("Log requested by " + req.ip);
          data = data.replace(/\n/g, '<br>\n');
          res.render('log', {logdata: data});
    }
    else 
    {
          console.log(err);
    }
  });
});

app.get('/manual/', function(req, res)
{
	console.log("Manual attempt requested by " + req.ip);
	if (req.query.source && req.query.currentAccession)
	{
		createManualAttempt(
			req.query.source,
			req.query.dest,
			req.query.priorAccession,
			req.query.currentAccession,
			req.query.priorSeries,
			req.query.currentSeries,
			req.query.BiasFieldCorrection,
			req.query.ExtractBrain,
			req.query.RegisterTo,
			req.query.ComparisonSettings,
			req.query.SliceType,
			req.query.CompareIncrease,
			req.query.CompareDecrease,
			req.query.GenerateHistogram,
			req.query.ResultsDicomSeriesDescription,
			(result)=>
		{
			res.render('manualcomplete', {message: result});
		});
	}
	else
	{
		let accession = req.query.accession;
		if (!accession) accession = "";
		res.render('manual2', {accession: accession});
	}
	
});


function createManualAttempt(
	source, dest, // AET settings
	priorAccession, currentAccession, priorSeries, currentSeries,// Series match
	BiasFieldCorrection, ExtractBrain, RegisterTo, ComparisonSettings, // compare settings
	SliceType, CompareIncrease, CompareDecrease, GenerateHistogram, ResultsDicomSeriesDescription, // output settings
	callback)
{
	// Clean out the descriptions
	if (currentAccession.indexOf(":") != -1)currentAccession = currentAccession.substring(0, currentAccession.lastIndexOf(":"));
	if (priorAccession.indexOf(":") != -1)priorAccession = priorAccession.substring(0, priorAccession.lastIndexOf(":"));
	if (priorSeries.indexOf(" (") != -1)priorSeries = priorSeries.substring(0, priorSeries.lastIndexOf(" ("));
	if (currentSeries.indexOf(" (") != -1)currentSeries = currentSeries.substring(0, currentSeries.lastIndexOf(" ("));	
			
	console.log(source);
	console.log(currentAccession);
	var options = 
	{
	  host: 'localhost',
	  path: '/' + source + '/study/byaccession/' + currentAccession,
	  port: config.LOCAL_PORT + 1,
	};
	
	DicomRequest(options, (out)=>
	{	
		try{results = JSON.parse(out);}catch{}
		
		// there should only be one result
		if (!results[0])
		{
			callback("ERROR: Could not find current accession.");
			return;
		}
	
		// Write recipe to file. 
		fs.readFile(config.DEFAULT_RECIPE, {encoding: 'utf-8'}, function(err,data)
		{
			if (!err) 
			{
				  var recipe = JSON.parse(data);
				  recipe.SourceAet = source;
				  recipe.OutputSettings.DicomDestinations = [dest];
				  recipe.PriorAccession = priorAccession;
				  recipe.CurrentAccession = currentAccession;
				  recipe.PriorSeriesCriteria[0].SeriesDescription = priorSeries;
				  recipe.PriorSeriesCriteria[0].SeriesDescriptionOperand = 0;
				  recipe.CurrentSeriesCriteria[0].SeriesDescription = currentSeries;
				  recipe.CurrentSeriesCriteria[0].SeriesDescriptionOperand = 0;
				  recipe.BiasFieldCorrection = BiasFieldCorrection == 'on';
				  recipe.ExtractBrain = ExtractBrain == 'on';
				  recipe.RegisterTo = RegisterTo == 'CURRENT' ? 1 : 0;
				  recipe.CompareSettings.CompareIncrease = CompareIncrease=='on';
				  recipe.CompareSettings.CompareDecrease = CompareDecrease=='on';
				  if (ComparisonSettings == 'Show all change')
				  {
					  recipe.CompareSettings.BackgroundThreshold = 1.0;
					  recipe.CompareSettings.MinRelevantStd = -999;
					  recipe.CompareSettings.MaxRelevantStd = 999;
					  recipe.CompareSettings.MinChange = 0;
					  recipe.CompareSettings.MaxChange = 999;
				  }
				  else
				  {
					  recipe.CompareSettings.BackgroundThreshold = 10.0,
					  recipe.CompareSettings.MinRelevantStd = -1;
					  recipe.CompareSettings.MaxRelevantStd = 5;
					  recipe.CompareSettings.MinChange = 0.8;
					  recipe.CompareSettings.MaxChange = 5;
				  }
				  recipe.CompareSettings.GenerateHistogram = GenerateHistogram == 'on';
				  //if (SliceType == 'SAGGITAL')recipe.OutputSettings.SliceType = 0;
				  //else if (SliceType == 'CORONAL')recipe.OutputSettings.SliceType = 1;
				  //else if (SliceType == 'AXIAL')recipe.OutputSettings.SliceType = 2;
				  recipe.OutputSettings.ResultsDicomSeriesDescription = ResultsDicomSeriesDescription;
				  
				  let output = 	JSON.stringify(recipe, null, 4);
				  fs.writeFileSync(config.MANUAL_CASE_PATH + currentAccession + ".json", output);
				  
				  callback("DONE! If the job doesn't appear in a few minutes have a look at the log.")
			}
			else 
			{
				  console.log(err);
				  callback("ERROR: Problem creating a recipe. Check that I have a default recipe and write access.")
				  return;
			}
		});
	});
}

app.get('/accessionsforpatient/:source?/:accession?', function(req, res)
{
	console.log("accessionsforpatient requested by " + req.ip);
	accessions = []
	var options = 
	{
	  host: 'localhost',
	  path: '/' + req.params.source + '/study/byaccession/' + req.params.accession,
	  port: config.LOCAL_PORT + 1,
	};
	
	// Make request for patient ID
	DicomRequest(options, (out)=>
	{	
		if (out == "failed")
		{
			res.status(500).send();
			return;
		}
		//if (!Array.isArray(out)) res.render('manualError');
		let patientId = '';
		results = JSON.parse(out);
		
		// there should only be one result
		if (!results[0])
		{
			res.status(500).send();
		}
		fields = results[0].split('\n');
		for (var i = 0; i < fields.length; i++)
		{
			if (fields[i].indexOf("(0010, 0020)") != -1)
			{
				patientId = fields[i].substring(fields[i].indexOf("'")+1).replace("'", "");
			}
		}
		options.path = "/" + req.params.source + '/study/bypatient/' + patientId;
		console.log("Patient id found " + patientId);
		
		DicomRequest(options, (out)=>
		{
			if (out == "failed")
			{
				res.status(500).send();
				return;
			}
			
			results = JSON.parse(out);
			
			for (var i = 0; i < results.length; i++)
			{
				let a = {number:"", description:"", date:"",};
				
				fields = results[i].split('\n');
				for (var j = 0; j < fields.length; j++)
				{
					if (fields[j].indexOf("(0008, 0050)") != -1)
					{
						a.number = fields[j].substring(fields[j].indexOf("'")+1).replace("'", "");
					}
					
					else if (fields[j].indexOf("(0008, 1030)") != -1)
					{
						a.description = fields[j].substring(fields[j].indexOf("'")+1).replace("'", "");
					}
					
					if (fields[j].indexOf("(0008, 0020)") != -1)
					{
						a.date = fields[j].substring(fields[j].indexOf("'")+1).replace("'", "");
					}
				}
				accessions.push(a);
			}
			
			console.log("Sending accessions " + patientId);
			accessions.reverse();
			res.json(accessions);
			res.end();
		});
		
	});
});

app.get('/seriesfor/:source?/:accession?', function(req, res)
{
	var options = 
		{
		  host: 'localhost',
		  path: '/' + req.params.source + '/study/byaccession/' + req.params.accession,
		  port: config.LOCAL_PORT + 1,
		};
		
	DicomRequest(options, (out)=>
	{
		if (out == "failed")
		{
			res.status(500).send();
			return;
		}
		results = JSON.parse(out);
		let uid = "";
		fields = results[0].split('\n');
		for (var j = 0; j < fields.length; j++)
		{
			if (fields[j].indexOf("(0020, 000d)") != -1)
			{
				uid = fields[j].substring(fields[j].indexOf(":")+2);
			}
		}
		
		options.path = '/' + req.params.source + '/series/bystudyuid/' + uid;
		console.log(options.path);
		DicomRequest(options, (out)=>
		{
			if (out == "failed")
			{
				res.status(500).send();
				return;
			}
			
			let series = [];
			results = JSON.parse(out);
			
			for (var i = 0; i < results.length; i++)
			{
				let s = {uid:"", description:"", count:""};
				fields = results[i].split('\n');
					for (var j = 0; j < fields.length; j++)
					{
						if (fields[j].indexOf("(0020, 000e)") != -1)
						{
							s.uid = fields[j].substring(fields[j].indexOf("'")+1).replace("'", "");
						}
						
						else if (fields[j].indexOf("(0008, 103e)") != -1)
						{
							s.description = fields[j].substring(fields[j].indexOf("'")+1).replace("'", "");
						}
						
						else if (fields[j].indexOf("(0020, 1209)") != -1)
						{
							s.count = fields[j].substring(fields[j].indexOf('"')+1).replace('"', '');
						}
					}
					series.push(s);
			}
			
			res.json(series);
			res.end();
		});
	});
	
});

function DicomRequest(options, onComplete)
{
	// This is a hack to handle async connection error if the server is down.
	// Because I don't know  where else to catch it (response.on('error') doesn't fire).
	// Setup a domain to catch all exceptions
	var d = require('domain').create()
	d.on('error', function(err)
	{
		console.log("RESTful Dicom service might be down...");
		onComplete("failed");
	});

	// Run the request inside that domain.
	d.run(()=>
	{
		let output = '';

		const request = http.get(options, (reponse) => 
		{
			const { statusCode } = reponse;
			
			let worked = true;
			reponse.setEncoding('utf8');
			reponse.on('error', (err) => {worked = false;});
			reponse.on('data', (chunk) => {output += chunk;});
			reponse.on('end', () => {if (statusCode == 200)onComplete(output);});
		});
		request.end();
	});
}

var server = app.listen(config.LOCAL_PORT, function () 
{
	console.log('Server is runnning...');
});
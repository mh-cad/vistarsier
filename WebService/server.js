var express = require('express'),
	sql = require('mssql'),
	fs = require('fs'),
	app = express();

app.set('view engine', 'pug');
app.use(express.static(__dirname + '/assets'));

// Fill these in:
const dbUser = 'user';
const dbPassword = 'password';
const dbServer = 'database server';
const logPath = 'path to capi logs\\CAPI.Service\\logs\\log.txt';

const sqlconfig = 
{
	user: '',
	password: '',
	server:'',
	database: 'Capi',
	parseJSON: true,
};

app.get('/', function (req, res)
{
	console.log("Dashboard requested by " + req.ip);
	res.render('dashboard');
});

app.get('/jobs/:lastid?', function (req, res) 
{
	console.log("Jobs requested by " + req.ip);
	
	var lastid = parseInt(req.params.lastid);

		sql.connect(sqlconfig, function(err)
		{
			if (err) console.log(err);
			
			var request = new sql.Request();
			var query = 'SELECT TOP(50) * from Jobs order by id desc';
			if (lastid) query = 'SELECT TOP(50) * from Jobs where id < ' + lastid + ' order by id desc'
			
			request.query(query, function(err, recordset)
			{
				if (err) console.log(err);
				text = " where ";
				
				var jobs = []
				
				for (i =0; i < recordset.recordsets[0].length; ++i)
				{
					record = recordset.recordsets[0][i];
					
					var time = new Date(record.End - record.Start);
					time = time.getMinutes() + ":" + ('0' + time.getSeconds()).slice(-2);
					
					if (record.Status == "Failed") time = "N/A";
					
					jobs.push({
						id: record.Id,
						name: record.PatientFullName,
						dob: record.PatientBirthDate,
						patientid: record.PatientId,
						current: record.CurrentAccession,
						prior: record.PriorAccession,
						source: record.SourceAet,
						dest: record.DefaultDestination,
						status: record.Status,
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

app.get('/cases/:lastid?', function (req, res) 
{
	console.log("Cases requested by " + req.ip);
	
	var lastid = parseInt(req.params.lastid);

		sql.connect(sqlconfig, function(err)
		{
			if (err) console.log(err);
			
			var request = new sql.Request();
			var query = 'SELECT TOP(50) * from Cases order by id desc';
			if (lastid) query = 'SELECT TOP(50) * from Cases where id < ' + lastid + ' order by id desc'
			
			request.query(query, function(err, recordset)
			{
				if (err) console.log(err);
				text = " where ";
				
				var cases = []
				
				for (i =0; i < recordset.recordsets[0].length; ++i)
				{
					record = recordset.recordsets[0][i];
					
					cases.push({
						id: record.Id,
						accession: record.Accession,
						status: record.Status,
						comment: record.Comment,
					});
					
					lastid = record.Id;
				}
			
				res.render('cases', {casedata : cases, lastid: lastid});
				
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
	fs.readFile(logPath, {encoding: 'utf-8'}, function(err,data){
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

var server = app.listen(5000, function () 
{
	console.log('Server is runnning...');
});
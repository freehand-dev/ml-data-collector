# ml-data-collector
> MlDataCollector - The simple data collector based on ASP.NET core Web API. Used to collect(categorization) data for Machine Learning.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-brightgreen.svg)](COPYING)
[![Build Status](https://dev.azure.com/oleksandr-nazaruk/ml-data-collector/_apis/build/status/freehand-dev.ml-data-collector)](https://dev.azure.com/oleksandr-nazaruk/ml-data-collector/_apis/build/status/freehand-dev.ml-data-collector)


## Compile and install
Once you have installed all the dependencies, get the code:

	git clone https://github.com/freehand-dev/ml-data-collector.git
	cd ml-data-collector

Then just use:

	sudo mkdir /opt/ml-data-collector/bin
	dotnet restore
	dotnet build
	sudo dotnet publish --runtime linux-x64 --output /opt/ml-data-collector/bin -p:PublishSingleFile=true -p:PublishTrimmed=true ./ml-data-collector

Install as daemon
   
	sudo nano /etc/systemd/system/ml-data-collector.service

The content of the file will be the following one

	[Unit]
	Description=MlDataCollector Service 

	[Service]
	Type=notify
	WorkingDirectory=/opt/ml-data-collector/etc/ml-data-collector
	Restart=always
	RestartSec=10
	KillSignal=SIGINT
	ExecStart=/opt/ml-data-collector/bin/ml-data-collector
	Environment=ASPNETCORE_ENVIRONMENT=Production 

	[Install]
	WantedBy=multi-user.target

Add daemon to startup

	sudo systemctl daemon-reload
	sudo systemctl start ml-data-collector
	sudo systemctl status ml-data-collector
	sudo systemctl enable ml-data-collector


## Configure and start
To start the server, you can use the `ml-data-collector` executable as the application or `sudo systemctl start ml-data-collector` as a daemon. For configuration you can edit a configuration file:

	sudo nano /opt/ml-data-collector/etc/ml-data-collector/ml-data-collector.conf

The content of the file will be the following one

	#
	[Logging:LogLevel]
	Default=Debug
	Microsoft=Warning

	#
	[Kestrel:Endpoints]
	Http:Url=http://*:58942
	Https:Url=https://*:58943
	Https:Certificate:Path=/opt/ml-data-collector/etc/ml-data-collector/cert.pfx
	Https:Certificate:Password=certpassword

## Docker

	$ docker pull oleksandrnazaruk/ml-data-collector:latest
	
	$ docker volume create ml-data-collector_data
	$ docker run --detach --name ml-data-collector --restart=always -v ml-data-collector_data:/opt/ml-data-collector/etc/ml-data-collector -p 58942:58942 -p 58943:58943 oleksandrnazaruk/ml-data-collector:latest
# Cities Autocomplete Service

## Table of Contents

1. [Description](#description)
2. [Technologies](#technologies)
3. [Initialization](#initialization)
4. [Usage](#usage)

----------

## Description

The current repository provides a simple autocomplete service while searching for cities of the world, as a free alternative to paid autocomplete features that is provided by third-party solutions.

The service is intended for applications in development phase, mainly for testing purposes. No performance optimazitions have been made for large scale applications, so you are adviced to use it at your own risk.

## Technologies

The service is based on the following techologies:

1. [Geonames Database](#database)
2. [Elasticsearch/Kibana](#elk)
3. [ASP.NET Core Minimal API](#api)
4. [Docker](#docker)

### Database

Cities of the world with population above 1000 are stored in a database provided by [Geonames](https://public.opendatasoft.com/explore/dataset/geonames-all-cities-with-a-population-1000/information/?disjunctive.cou_name_en&sort=name). The database contains more than 140.000 cities and the specifications can be found in the link above. A cool feature is that there are alternative names provided for each city (eg. local names, names in different languages etc), giving flexibility on the name that a user can search for.

### Elasticsearch/Kibana

The city database is inserted in CSV format to Elasticsearch, using the *File Data Visualizer* of Kibana.

In order to be compatible with the naming conventions of Elasticsearch, the names of the fields have been slightly altered. Also, the types of fields containing the names of the cities have been changed to "search-as-you-type", providing fast response times but increasing the overall footprint.

Please note that Elasticsearch and Kibana are used with no authentication in place, so use at your own risk.

The version used is v8.3.3.

### ASP.NET Core Minimal API

The intended interface with Elasticsearch is through a REST API, which is written in .NET6 and [ASP.NET Core Minimal API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0&tabs=visual-studio). The API communicates with Elasticsearch via the official [.NET client](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/introduction.html) and [NEST](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/nest.html) high-level library. It can be consumed by client-side applications via regular AJAX calls.

For the seamless integration to Docker, a Dockerfile has been generated automatically during the solution creation in [Visual Studio 2022](https://docs.microsoft.com/en-us/visualstudio/containers/overview?view=vs-2022), along with the corresponding Docker Compose file, as described in the link above.

### Docker

Both Elasticsearch/Kibana and the API are containerized using [Docker Compose](https://docs.docker.com/compose/). This allows the individual containers to be run as a single application with a shared network. The "docker-compose.yml" file is included in the repository.

The Elasticsearch, Kibana and .NET6 images are pulled from their respective official sources, while the API solution is built from its Dockerfile.

The OS for the container is set to Linux.

## Initialization

The following steps describe the way to run the service:

### 1. Install Docker Desktop

The Docker Desktop can be downloaded [here](https://docs.docker.com/desktop/), by clicking the "Install" links on the sidebar. Please make sure to download the version for your OS and follow the instructions carefully.

### 2. Download the contents of the repository

Click Code -> Download Zip to download the files of the repository. Extract the .zip file contents (you will need the folder path later).

### 3. Open Docker Desktop

Open Docker Desktop, which you downloaded earlier. **Make sure that Docker Desktop is up and running while using the service, as it wont operate otherwise.**

### 4. Run Docker Compose

Open a command prompt and navigate to the folder containing the files of the repository that you downloaded earlier. Use the following command:

`cd $Path to the folder$`

The folder contains the "docker-compose.yml" file needed to initialize the service. Run the following command to start Docker Compose:

`docker-compose up -d`

This should start the procedure of pulling the neccessary images and setting up the containers. **The images that will be downloaded have a total size of ~2.3GB.**

![Images being pulled](/media/image_pull.png)

### 5. Check if the service is ready

After the images have been pulled, the individual containers will be initialized.

![Container initialization](/media/initialization.png)

Open Docker Desktop and click on the "Container" tab. You should be able to see a new Docker application with three containers: elasticsearch, kibana and api.

![Containers in Docker](/media/containers.png)

Elasticsearch will take a few minutes to finish initialization. **The service will not run until everything has finished initializing.**

To check if the service is ready, click the Kibana container to open its terminal window. The service is ready when you can see the following message in the terminal:

`[INFO ][status] Kibana is now available (was degraded)`

and you can successfully open the Kibana in your browser.

![Kibana in browser](/media/kibana.png)

## Usage

The API provides the two following endpoints:

1. `http://localhost:8000/clusterhealth`
2. `http://localhost:8000/autocomplete`

### 1. Clusterhealth

The first endpoint is useful to determine if the cluster is alive. Using a GET method, a typical response in which the cluster is up and running is as the following:

```
{
  "active_primary_shards": 10,
  "active_shards": 10,
  "active_shards_percent_as_number": 90.9090909090909,
  "cluster_name": "docker-cluster",
  "delayed_unassigned_shards": 0,
  "indices": null,
  "initializing_shards": 0,
  "number_of_data_nodes": 1,
  "number_of_in_flight_fetch": 0,
  "number_of_nodes": 1,
  "number_of_pending_tasks": 0,
  "relocating_shards": 0,
  "status": "yellow",
  "task_max_waiting_in_queue": null,
  "task_max_waiting_in_queue_millis": 0,
  "timed_out": false,
  "unassigned_shards": 1
}
```

### 2. Autocomplete

In order to implement the autocomplete feature, the second endpoint is used with a GET method. The endpoint requires a query param with the term to search for. For example, in order to receive the possible autocomplete candidates for cities starting with "Ath", the following URL is used:

`http:localhost/autocomplete?input=Ath`

The response consists of a JSON with up to 5 cities. The output format is:

```
[
  {
    "geonameID": 0,
    "asciiName": "string",
    "countryNameEN": "string",
    "coordinates": {
      "longitude": 0,
      "latitude": 0
    }
  }
]
```

The internal query to Elasticsearch is made in a way that the results appear with descending order of relevance, weighted by each city's population. As a result, cities with greater population will tend to appear higher in the results. If you wish, you can override this behaviour by altering the query in the API.

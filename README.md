# gRPC Service

- [Encriptado](https://github.com/balalofernandez/gRPC-example/blob/e23abf35526e1b518e1164d8cdaf66be5e247cfa/encryption.md)

## Protocol Buffers

En gRPC se usan los [protocol buffers](https://developers.google.com/protocol-buffers/docs/overview), estos describen la interfaz del servicio y los mensajes de carga:

```
service HelloService {
  rpc SayHello (HelloRequest) returns (HelloResponse);
}

message HelloRequest {
  string greeting = 1;
}

message HelloResponse {
  string reply = 1;
}
```

En los protocol buffers, tenemos que describir los servicios (`service`) y los tipos de objetos que se van a pasar (`message`).

### Message

Los `message` contienen una serie de campos que pueden ser `optional`, `required` o `repeated` seguido del tipo de campo, el nombre y un numero. Este número es identificador, los números del 1-16 usan 1 bit, 16-2047 usan 2, etc.

### Generación de código de cliente y servidor.

A partir de la definición del fichero `.proto` y con la ayuda de GRPC.Tools (el paquete de NuGet) podemos crear los siguientes ficheros que se generaran en una en la carpeta `Debug`. Generaremos dos archivos por cada fichero `.proto`:
- `Ejemplo.cs` que contiene el código del protocol buffer para completar, serializar y recuperar nuestros tipos de mensajes de solicitud y respuesta.
- `EjemploGrpc.cs` que nos proporciona las clases servidor y clientes generadas:
	+ una clase abstracta `Ejemplo.EjemploBase` para heredar al definir las implementaciones del servicio Ejemplo
	+ una clase `Ejemplo.EjemploClient` que se puede usar para acceder a instancias remotas de Ejemplo.


Para generar correctamente los ficheros mencionado anteriormente tenemos que:
1. Dentro de las propiedades del proyecto, incluir la ruta al fichero proto de la siguiente manera: (GrpcServices sirve para declarar que hay un servicio asociado al protocolbuffer, en el siguiente caso se indica que es un _Server_ pero también puede ser _Client_ o _Both_)
	```
	<ItemGroup>
		<Protobuf Include="Protos\Ejemplo.proto" GrpcServices="Server" />
	</ItemGroup>
	```
2. En segundo lugar hay que referenciar los paquetes Nuget Grpc.Tools y Grpc.Core
	```
    <PackageReference Include="Grpc.AspNetCore" Version="2.42.0" />
    <PackageReference Include="Grpc.Tools" Version="2.44.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
	```
## Crear el Servidor

Existen dos partes necesarias para que el servicio haga su trabajo:
- Implementar la funcionalidad del servicio al heredar de la clase base obtenida a partir de nuestra definición: hacer el "trabajo" real de nuestro servicio.
- Ejecutar un servidor gRPC para escuchar las solicitudes de los clientes y devolver las respuestas del servicio. 

En primer lugar, la clase tiene que heredar de la clase generada:
```
public class EjemploImpl : Ejemplo.EjemploBase
```

Recordemos que en los servicios RPC convivían varios tipos:
- El RPC simple, al cual se le pasa el objeto definido en el archivo `.proto` y devuelve otro objeto también predefinido. Además se le pasa un contexto del servidor:
	```
    public override Task<Feature> GetFeature(Point request, Grpc.Core.ServerCallContext context)
	{
		return Task.FromResult(CheckFeature(request));
	}
	```
- Por otro lado podíamos abrir diversas transmisiones (Unidireccional o bidireccional). En este caso pasaremos un requestStream, un responseStream y un contexto:
	```
	public override async Task RouteChat(Grpc.Core.IAsyncStreamReader<RouteNote> requestStream,
		Grpc.Core.IServerStreamWriter<RouteNote> responseStream,
		Grpc.Core.ServerCallContext context)
		{
			while (await requestStream.MoveNext())
			{
				var note = requestStream.Current;
				List<RouteNote> prevNotes = AddNoteForLocation(note.Location, note);
				foreach (var prevNote in prevNotes)
				{
					await responseStream.WriteAsync(prevNote);
				}
			}
		}
	```

Finalmente iniciaremos el servidor:
	```
	var features = RouteGuideUtil.LoadFeatures();

	Server server = new Server
	{
		Services = { RouteGuide.BindService(new RouteGuideImpl(features)) },
		Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
	};
	server.Start();

	Console.WriteLine("RouteGuide server listening on port " + port);
	Console.WriteLine("Press any key to stop the server...");
	Console.ReadKey();

	server.ShutdownAsync().Wait();
	```
	
## Crear el Cliente

Usando la clase cliente que se genera a partir de archivo `.proto` se tiene lo siguiente:

	```
	Channel channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
	var client = new RouteGuide.RouteGuideClient(channel);

	// YOUR CODE GOES HERE

	channel.ShutdownAsync().Wait();
	```

También hay que diferenciar los casos de las llamadas RPC (simple y transmisión):
- Dentro del simple se pueden distinguir dos tipos, síncrono y asíncrono:
```
Point request = new Point { Latitude = 409146138, Longitude = -746188906 };
Feature feature = client.GetFeature(request);
//Async
Point request = new Point { Latitude = 409146138, Longitude = -746188906 };
Feature feature = await client.GetFeatureAsync(request);
```
- La transmisión será de la siguiente forma:
```
using (var call = client.RouteChat())
{
    var responseReaderTask = Task.Run(async () =>
    {
        while (await call.ResponseStream.MoveNext())
        {
            var note = call.ResponseStream.Current;
            Console.WriteLine("Received " + note);
        }
    });

    foreach (RouteNote request in requests)
    {
        await call.RequestStream.WriteAsync(request);
    }
    await call.RequestStream.CompleteAsync();
    await responseReaderTask;
}
```

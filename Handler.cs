using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace otto;

enum Request
{
    Default = 0
};

enum Definition
{
    Default = 0
};

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
struct Data
{
    [MarshalAs(UnmanagedType.R8)]
    public double longitude;
    
    [MarshalAs(UnmanagedType.R8)]
    public double latitude;

    [MarshalAs(UnmanagedType.R8)]
    public double heading;

    [MarshalAs(UnmanagedType.R8)]
    public double altitude;

    [MarshalAs(UnmanagedType.R8)]
    public double speed;
}

public class Handler
{
    private SimConnect? client = null;

    public void Connect()
    {
        try
        {
            client = new SimConnect("", 0, 0, null, 0);

            client.OnRecvOpen += new SimConnect.RecvOpenEventHandler(OnRecvOpenEvent);
            client.OnRecvQuit += new SimConnect.RecvQuitEventHandler(OnRecvQuitEvent);
            client.OnRecvException += new SimConnect.RecvExceptionEventHandler(OnRecvExceptionEvent);
            client.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(OnRecvSimObjectDataEvent);
            
            client.AddToDataDefinition(Definition.Default, "PLANE LONGITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            client.AddToDataDefinition(Definition.Default, "PLANE LATITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            client.AddToDataDefinition(Definition.Default, "PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            client.AddToDataDefinition(Definition.Default, "PLANE ALTITUDE", "Feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            client.AddToDataDefinition(Definition.Default, "AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            client.RegisterDataDefineStruct<Data>(Definition.Default);

            client.RequestDataOnSimObject(Request.Default, Definition.Default, 0, SIMCONNECT_PERIOD.SECOND, 0, 0, 1, 0);

            while (client != null)
            {
                client.ReceiveMessage();
                Thread.Sleep(1000);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        client?.Dispose();
        client = null;
    }

    private void OnRecvOpenEvent(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        Console.WriteLine("CONNECTED TO SIM");
    }

    private void OnRecvQuitEvent(SimConnect sender, SIMCONNECT_RECV data)
    {
        Console.WriteLine("DISCONNECTED FROM SIM");
    }

    private void OnRecvExceptionEvent(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
        SIMCONNECT_EXCEPTION exception = (SIMCONNECT_EXCEPTION)data.dwException;
        Console.WriteLine($"ERROR: {exception}");
    }

    private void OnRecvSimObjectDataEvent(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        Data result = (Data)data.dwData[0];
        Console.WriteLine($"LON: {result.longitude}, LAT: {result.latitude}, HDG: {result.heading}, ALT: {result.altitude}, SPD: {result.speed}");
    }
}

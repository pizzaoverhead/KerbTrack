using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


class OpentrackUdpTracker : ITracker
{
    protected Vector3 rotationState;
    protected Vector3 positionState;
    protected UdpClient client = null;
    protected IPEndPoint clientEndPoint;
    protected IPEndPoint serverEndPoint;
    protected bool runThread = false;
    protected Thread worker = null;
    protected readonly object updateLock;

    public OpentrackUdpTracker()
    {
        updateLock = new object();
        Start();
    }

    public void GetData(ref Vector3 rot, ref Vector3 pos)
    {
        lock(this.updateLock)
        {
            rot = rotationState;
            pos = positionState;
        }
    }

    public void ResetOrientation() { }

    public void Start()
    {
        // TODO configurable port and limit server ip
        this.Stop();
        this.serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
        this.clientEndPoint = new IPEndPoint(IPAddress.Any, 4242);
        this.client = new UdpClient(this.clientEndPoint);
        this.runThread = true;
        this.worker = new Thread(Worker);
        this.worker.Start();
    }

    public void Stop()
    {
        this.runThread = false;
        if(this.client != null)
            this.client.Close();  // will stop worker blocking on recieve

        // Make sure the thread is finished before we can call start again
        if(this.worker != null)
            this.worker.Join();
    }

    protected void Worker()
    {
        System.Threading.Thread.CurrentThread.IsBackground = true;

        while(this.runThread)
        {
            try
            {
                byte[] datagram = this.client.Receive(ref this.serverEndPoint);
                using(var inputStream = new MemoryStream(datagram))
                {
                    using(var reader = new BinaryReader(inputStream))
                    {
                        lock(this.updateLock)
                        {
                            // Datagram just contains 6 doubles, (x, y, z, yaw, pitch, roll).
                            // KerbTrack seems to want translate values between -0.5 and +0.5.
                            // Opentrack outputs translate values between -75 and +75 by default
                            // (highly configurable of course).
                            this.positionState.x = (float)(reader.ReadDouble() / 150.0);
                            this.positionState.y = (float)(reader.ReadDouble() / 150.0);
                            this.positionState.z = (float)(reader.ReadDouble() / 150.0);
                            this.rotationState.y = (float)reader.ReadDouble();  // y is yaw
                            this.rotationState.x = (float)reader.ReadDouble();  // x is pitch
                            this.rotationState.z = (float)reader.ReadDouble();  // z is roll
                        }
                    }
                }
            }
            catch(System.ObjectDisposedException)
            {
                return;  // socket closed, bail on the worker thread
            }
            catch(SocketException e)
            {
                // TODO log it
                if(this.runThread)
                    System.Threading.Thread.Sleep(0); // `Yield` for dotnet 3.5
            }
        }
    }
}

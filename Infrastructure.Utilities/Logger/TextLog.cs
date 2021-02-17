using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace F2X.Interoperabilidad.Operador.Infrastructure.Helper.Utilities.Logger
{
    public class TextLog
    {
        /// <summary>
        /// FileName: Es el nombre del archivo para el log de eventos
        /// Active: Variable para activar la escritura del log.
        /// </summary>
        public string FileName { get; set; }
        public bool Active { get; set; }
        public string DirectoryLog { get { return Path.Combine(Directory.GetCurrentDirectory(), "Log");  } }

        public ConcurrentQueue<DatosLog> ColaLogs { get; set; }
        protected CancellationTokenSource _cancellationTokenSourceEscribirLog;
        protected CancellationToken _cancellationTokenEscribirLog;
        protected Task _escribirLog;
        /// <summary>
        /// Evento para enviar el rastro de errores generados al escribir el archivo
        /// </summary>
        /// <param name="data"></param>
        public delegate void SubscribeEventError(string data);
        public event SubscribeEventError EventError;

        /// <summary>
        /// Delegado utilizado para la escritura del archivo
        /// </summary>
        /// <param name="logdata"></param>
        /// <param name="estacion"></param>
        /// <param name="carril"></param>
        delegate void DelegateEscribirLog(string logdata, string carril);

        /// <summary>
        /// Objeto que protege la funcion para encolar los llamados y atenderlos uno a uno.
        /// </summary>
        private readonly object _writeAsync;       

        public TextLog()
        {
            _writeAsync = new object();
            ColaLogs = new ConcurrentQueue<DatosLog> { };
            _cancellationTokenSourceEscribirLog = new CancellationTokenSource();
            _cancellationTokenEscribirLog = _cancellationTokenSourceEscribirLog.Token;
            _escribirLog = Task.Factory.StartNew(() => { EscribirDesdeCola(); }, _cancellationTokenEscribirLog);
        }

        ~TextLog()
        {
            _cancellationTokenSourceEscribirLog.Cancel();
        }

        /// <summary>
        /// Funcion para la escritura del log por medio de un delegado.
        /// </summary>
        /// <param name="logdata"></param>
        public void EscribirLog(string logdata, string carril)
        {
            if (!Active) return;

            if (string.IsNullOrEmpty(FileName))
                return;

            ColaLogs.Enqueue(new DatosLog { logdata = logdata, carril = carril, fecha = DateTime.Now });
        }

        /// <summary>
        /// Funcion para la escritura del log por medio de un delegado.
        /// </summary>
        /// <param name="logdata"></param>
        public void EscribirLog(DateTime fechaLog, string logdata, string carril= null)
        {
            if (!Active) return;

            if (string.IsNullOrEmpty(FileName))
                return;

            ColaLogs.Enqueue(new DatosLog { logdata = logdata, carril = carril, fecha = fechaLog });
        }    

        /// <summary>
        /// Funcion que escribe los datos en una linea de un archivo de texto.
        /// </summary>
        /// <param name="logdata"></param>
        /// <param name="estacion"></param>
        /// <param name="carril"></param>
        private void Escribir(string logdata, string carril)
        {
            lock (_writeAsync)
            {
                try
                {
                    var time = DateTime.Now;                    
                    var newpath =  CreatePathLog(DirectoryLog, FileName);
                    
                    var writer = File.AppendText(newpath);
                    writer.WriteLine(time.ToString("yyyy/MM/dd HH:mm:ss.fff") + ";" + logdata);
                    writer.Close();
                }
                catch (Exception ex)
                {
                    EventError?.Invoke(MethodBase.GetCurrentMethod().Name + ex);
                }
            }
        }       

        private void EscribirDesdeCola()
        {
            try
            {
                DatosLog log;
                while (!_cancellationTokenEscribirLog.IsCancellationRequested)
                {
                    if (!ColaLogs.TryDequeue(out log))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    if (log == null) continue;
                    var newPath = CreatePathLog(DirectoryLog, FileName);
                    var writer = File.AppendText(newPath);
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")};{log.fecha.ToString("yyyy/MM/dd HH:mm:ss.fff")};{log.logdata}");
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(MethodBase.GetCurrentMethod().Name + ex);
            }
        }

        public string CreatePathLog(string pathRoot, string fileName)
        {
            var time = DateTime.Now;
            var newPath = pathRoot;

            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            newPath = Path.Combine(newPath, time.Year.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            newPath = Path.Combine(newPath, time.Month.ToString("D2"));
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            newPath = Path.Combine(newPath, time.Day.ToString("D2"));
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            newPath = Path.Combine(newPath, $"{fileName}{time.ToString("_yyyy_MM_dd")}.log");
            return newPath;
        }
    }

    public class DatosLog
    {
        public string logdata { get; set; }
        public string carril { get; set; }
        public DateTime fecha { get; set; }
    }
}

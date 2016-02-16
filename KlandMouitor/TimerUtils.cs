using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace KlandMouitor
{
    class TimerUtils
    {
        static Form1 form;
        static System.Timers.Timer timer = null;
        static string logPath = @"c:\kland\";
        static int time = 10;
        static int timeStr = 10;
        static string serviceName = "";
        static string type = "";
        static string appName = "KlandMouitor";
        static long logFileMaxSize = 10485760;//10Mb
        static long mouitorCount = 0;

        public static void initTimer(Form1 form1,string ServiceName, int Time, string Type)
        {
            writeLog("初始化参数");
            if (form1 != null)
            {
                form = form1;
            }
            if (ServiceName != null && ServiceName.Length > 0)
            {
                serviceName = ServiceName;
            }
            if (Type != null && Type.Length > 0)
            {
                type = Type;
            }
            if (Time > 0)
            {
                timeStr = Time;
                time = checkType(Time, type);
            }
            mouitorCount = 0;
            form.setMouitorCount(0);
        }

        private static int checkType(int Time, string type)
        {
            int typeSize = 1000;
            if (type != null && type.Length > 0)
            {

                switch (type)
                {
                    case "秒":
                        typeSize = 1000;
                        break;
                    case "分钟":
                        typeSize = 60 * 1000;
                        break;
                    case "小时":
                        typeSize = 60 * 60 * 1000;
                        break;
                    case "天":
                        typeSize = 24 * 60 * 60 * 1000;
                        break;
                    default:
                        break;

                }
            }
            return Time * typeSize;

        }

        public static int startTimer()
        {
            int flag = _checkService();
            try
            {
                if (flag == -1)
                {
                    MessageBox.Show("要监控的服务[" + serviceName + "]不存在！");
                    writeLog("要监控的服务[" + serviceName + "]不存在！");
                    form.stopMouitor();
                    return flag;
                }
                else if(flag == 0)
                {
                    writeLog("要监控的服务[" + serviceName + "]还未启动");
                    writeLog("启动要监控的服务[" + serviceName + "]开始");
                    ServiceUtils.StartService(serviceName);
                    writeLog("启动要监控的服务[" + serviceName + "]完成");
                }

                if (time < 1)
                {
                    time = 10;
                }
                writeLog("初始化定时器");
                writeLog("监控服务[" + serviceName + "]的间隔时间为：" + timeStr + type);
                setAutoRun(Application.ExecutablePath, true);
                timer = new System.Timers.Timer(time);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(checkService);
                timer.AutoReset = true;
                timer.Enabled = true;
            }
            catch (Exception e)
            {
                writeLog("启动定时器时出现异常：" + e.ToString());
            }
            saveConfig();

            return flag;

        }

        public static void stopTimer()
        {
            if (timer != null)
            {
                try
                {
                    timer.Enabled = false;
                    timer = null;
                }
                catch (Exception e)
                {
                    writeLog("停止定时器时出现异常：" + e.ToString());
                }
            }
        }

        private static void checkService(object sender, System.Timers.ElapsedEventArgs ee)
        {
            try
            {
                writeLog("--------------------监控服务开始--------------------");
                int flag = _checkService();
                bool isExist = ServiceUtils.IsServiceIsExisted(serviceName);
                if (flag == 1)
                {
                    writeLog("服务[" + serviceName + "]已经启动");
                }
                else if(flag == 0)
                {
                    writeLog("服务[" + serviceName + "]未启动");
                    writeLog("启动服务[" + serviceName + "]开始");
                    ServiceUtils.StartService(serviceName);
                    writeLog("启动服务[" + serviceName + "]完成");
                }
                else if (flag == -1)
                {
                    MessageBox.Show("服务[" + serviceName + "]不存在！");
                    writeLog("服务[" + serviceName + "]不存在！");
                    stopTimer();
                    form.stopMouitor();
                }
                writeLog("--------------------监控服务结束--------------------");
                form.setMouitorCount(mouitorCount++);
            }
            catch (Exception e)
            {
                writeLog("监控服务时出现异常：" + e.ToString());
            }

        }

        private static int _checkService()
        {
            //-1:不存在；1:已启动；0:未启动
            int flag = 0;
            try
            {
                bool isExist = ServiceUtils.IsServiceIsExisted(serviceName);
                if (isExist)
                {
                    bool isStart = ServiceUtils.IsServiceStart(serviceName);
                    if (isStart)
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }
                }
                else
                {
                    flag = -1;
                }
            }
            catch (Exception e)
            {
                writeLog("监控服务时出现异常：" + e.ToString());
            }
            return flag;
        }

        public static void writeLog(string msg)
        {

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                string logFilePath = logPath + "\\KlandMouitorService-log.txt";
                if (File.Exists(logFilePath))
                {
                    FileInfo logFileInfo = new FileInfo(logFilePath);
                    long logFileSize = logFileInfo.Length;
                    if (logFileSize > logFileMaxSize)
                    {
                        Compress(logFilePath, logPath + "\\KlandMouitorService-log-" + DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds + ".gz");
                        File.Delete(logFilePath);
                    }
                }
                else
                {
                    File.Create(logFilePath);
                }

                fs = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(DateTime.Now.ToString() + " " + msg + "\n");
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("写日志时出现异常：" + e.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }

        }

        public static void Compress(string fileName,string destFile)
        {

            MemoryStream ms = new MemoryStream();

            GZipStream compressedStream = new GZipStream(ms, CompressionMode.Compress, true);
            FileStream fs = new FileStream(fileName, FileMode.Open);
            byte[] buf = new byte[1024*64];
            int count = 0;
            do
            {
                count = fs.Read(buf, 0, buf.Length);
                compressedStream.Write(buf, 0, count);
            }
            while (count > 0);

            try
            {
                fs.Close();
                compressedStream.Close();
                File.WriteAllBytes(destFile, ms.ToArray());
            }
            catch (Exception e)
            {
                writeLog("压缩日志文件[" + destFile + "]时出现异常：" + e.ToString());
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                    fs.Close();
                }
                if (compressedStream != null)
                {
                    compressedStream.Dispose();
                    compressedStream.Close();
                }

            }

        }

        public static void setAutoRun(string filePath, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    writeLog("该文件不存在：" + filePath);
                    return;
                }
                String name = Path.GetFileNameWithoutExtension(filePath);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                {
                    if (reg.GetValue(name) != null && reg.GetValue(name).ToString().ToLower() != "false")
                    {
                        writeLog("自启动已存在，无需设置");
                    }
                    else
                    {
                        reg.SetValue(name, "\"" + filePath + "\" /start");
                        writeLog("自启动设置完成");
                    }
                }
                else
                {
                    reg.SetValue(name, false);
                    writeLog("自启动取消成功，无需设置");
                }
            }
            catch (Exception e)
            {
                writeLog("设置自启动时出现异常：" + e.ToString());
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }

        public static string[] readConfigFromFile()
        {
            string[] configArr = null;
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string configPath = appDataPath + "\\" + appName;

            DirectoryInfo info = new DirectoryInfo(configPath);
            FileInfo[] files = info.GetFiles();
            FileInfo newsFile = null;

            foreach (FileInfo f in files)
            {
                if (newsFile == null)
                {
                    newsFile = f;
                }
                if (f.LastWriteTime > newsFile.LastWriteTime)
                {
                    if (f.Name.EndsWith("-config.txt"))
                    {
                        newsFile = f;
                    }
                }
            }

            configPath += "\\" + newsFile.Name;

            if (File.Exists(configPath))
            {
                StreamReader sr = null;
                String configStr = "";
                try
                {
                    sr = new StreamReader(configPath, System.Text.Encoding.UTF8);
                    configStr = sr.ReadToEnd().ToString();
                    sr.Dispose();
                    sr.Close();
                }
                catch (Exception e)
                {
                    writeLog("读取配置文件时出现异常：" + e.ToString());
                }
                finally
                {
                    if (sr != null)
                    {
                        sr.Dispose();
                        sr.Close();
                    }
                }
                configStr = Regex.Replace(configStr, @"[\n\r]", "");
                if (!string.IsNullOrEmpty(configStr))
                {
                    configArr = configStr.Split('|');
                    if (configArr.Length != 3)
                    {
                        configArr = null;
                    }
                }
            }
            return configArr;
        }

        public static void saveConfig()
        {
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string configPath = appDataPath + "\\" + appName;
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                configPath += "\\" + serviceName + "-config.txt";
                fs = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(serviceName + "|" + timeStr + "|" + type);
                sw.Flush();
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            catch (Exception e)
            {
                writeLog("保存配置文件时出现异常：" + e.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
                if (fs != null)
                {
                    fs.Dispose();
                    fs.Close();
                }

            }

        }


    }
}

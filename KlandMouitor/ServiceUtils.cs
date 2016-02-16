using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using System.Collections;
using Microsoft.Win32;

namespace KlandMouitor
{
    class ServiceUtils
    {

        /// 检查服务存在的存在性
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>存在返回 true,否则返回 false;</returns>
        public static bool IsServiceIsExisted(string NameService)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == NameService.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断某个Windows服务是否启动
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceStart(string serviceName)
        {
            ServiceController psc = new ServiceController(serviceName);
            bool bStartStatus = false;
            try
            {
                if (!psc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    bStartStatus = true;
                }

                return bStartStatus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>  
        /// 修改服务的启动项 2为自动,3为手动  
        /// </summary>  
        /// <param name="startType"></param>  
        /// <param name="serviceName"></param>  
        /// <returns></returns>  
        public static bool ChangeServiceStartType(int startType, string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                servicesName.SetValue("Start", startType);
            }
            catch (Exception ex)
            {
                TimerUtils.writeLog("更改服务启动类型时出现异常：" + ex.ToString());
                return false;
            }
            return true;


        }



        public static bool StartService(string serviceName)
        {
            bool flag = true;
            if (IsServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running && service.Status != System.ServiceProcess.ServiceControllerStatus.StartPending)
                {
                    try
                    {
                        service.Start();
                        for (int i = 0; i < 60; i++)
                        {
                            service.Refresh();
                            System.Threading.Thread.Sleep(1000);
                            if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                            {
                                break;
                            }
                            if (i == 59)
                            {
                                flag = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TimerUtils.writeLog("启动服务时出现异常：" + e.ToString());
                    }

                }
            }
            return flag;
        }


        public static bool StopService(string serviceName)
        {
            bool flag = true;
            if (IsServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    service.Stop();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            break;
                        }
                        if (i == 59)
                        {
                            flag = false;
                        }
                    }
                }
            }
            return flag;
        }

        public static List<string> getAllService()
        {
            List<string> serviceList = new List<string>();
            ServiceController[] services = System.ServiceProcess.ServiceController.GetServices();
            for (int i = 0; i < services.Length; i++)
            {
                serviceList.Add(services[i].ServiceName);
                Console.WriteLine(services[i].ServiceName);
            }
            Console.WriteLine(serviceList.Count);
            return serviceList;
        }


    }
}

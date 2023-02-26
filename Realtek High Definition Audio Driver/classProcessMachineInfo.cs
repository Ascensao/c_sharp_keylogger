using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Driver_Foundation
{
    class classProcessMachineInfo
    {
        private static classMySQLDatabase classMysql;
        private static classMachineInformation classMachineInfo;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_machineInfo_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classMachineInfo = new classMachineInformation();
            classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();

            int order_status = classMysql.getMachineInfoStatus(uid); //se a máquina nunca foi registada o metdo Mysql retorna -1

            /* tbl_machine_info GUIDE
             * id
             * client_id
             * machine_name
             * arq
             * username
             * user_home_folder
             * windows
             * version
             * webbrowser
             * macaddress
             * regist_date
             * order_status */

            if (order_status == -1 || order_status == 1)
            {
                switch (order_status)
                {
                    case -1:
                        classExeption.printConsole("execute_machineInfo_protocol()", "Maquina não registada.");
                        classMysql.insert_machine_info(uid);
                        classExeption.printConsole("execute_machineInfo_protocol()", "Maquina registada com sucesso.");
                        break;
                    case 1:
                        classExeption.printConsole("execute_machineInfo_protocol()", "Maquina já registada.");
                        break;
                }
            }

            classMysql.closeDatabaseConnection();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace Innovaction
{
  /// <summary>
    /// Clase generada para facilitar el mantenimiento de los WS de Farmatodo
  /// </summary>
    public static class WSManager
    {
       /// <summary>
       /// Para que esto funcione tiene que estar enablePasswordRetrieval="true" y passwordFormat="Encrypted". 
       /// Los usuarios   
       /// </summary>
       /// <param name="PortalID"> El portal ID donde esta el user</param>
       /// <param name="UserID"> El userID del cual queres la password</param>
       /// <returns></returns>
       private static string GetPassword(int PortalID, int UserID)
        {
            if (UserID == -1){ return string.Empty;} // No esta logueado
            try
            {
                DotNetNuke.Entities.Users.UserInfo uInfo = DotNetNuke.Entities.Users.UserController.GetUserById(PortalID, UserID);
                string password = DotNetNuke.Entities.Users.UserController.GetPassword(ref uInfo, String.Empty);
                return password;
            }
                
               
            catch (Exception Ex)
            {
                throw new Exception("Exception at WSManager GetPassword. Exception says: " + Ex.Message, Ex);
            }
        }

       /// <summary>
       /// Llama al metodo CustomerDataWS.validateLogin usando los datos que saca del UserID y PortalID
       /// </summary>
       /// <param name="PortalID">El PortalID donde esta el User</param>
       /// <param name="UserID">El UserId *DEL NUKE* del cual querés validar la info.</param>
       /// <returns></returns>
       public static CustomerDataWS.customerResponse ValidateLoginByUserID(int PortalID, int UserID)
        {
            
            try
            {
                string password = GetPassword(PortalID: PortalID, UserID: UserID);
                string nickname = DotNetNuke.Entities.Users.UserController.GetUserById(userId: UserID, portalId: PortalID).Email;
                
                var Result = ValidateLogin(nickname,password, PortalID);

                return Result;
            }

            catch (Exception Ex)
            {
                throw new Exception("Hubo un error al tratar de validar el usuario con validateLogin en el metodo ValidateLoginByUserID. La excepcion dice: " + Ex.Message, Ex.InnerException);
            }
        }
      
       public static CustomerDataWS.customerResponse ValidateLogin(string nickname, string password, int PortalID)
        {
            
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var cliente = new CustomerDataWS.CustomerDataWSClient();

                var ToSearch = new CustomerDataWS.customerRequest();


                ToSearch.customer = new CustomerDataWS.customerTo();
                ToSearch.customer.country = new CustomerDataWS.countryTo();
                ToSearch.customer.country.id = CountryID(PortalID);

                ToSearch.source = new CustomerDataWS.sourceTo();
                ToSearch.source.id = "WEB";
                ToSearch.customer.password = password;
                ToSearch.customer.nickname = nickname;


                var Result = cliente.validateLogin(ToSearch);

                return Result;
            }

            catch (Exception Ex)
            {
                throw new Exception("Hubo un error al tratar de validar el usuario con validateLogin en el metodo ValidateLogin. La excepcion dice: " + Ex.Message, Ex.InnerException);
            }
        }
        

       // se podrian quitar si queremos
       public static CustomerDataWS.customerResponse SearchFullCustomer(string nationalID, int PortalID)
        {
            return SearchCustomer(nationalID, PortalID, false);
        }
       public static CustomerDataWS.customerResponse SearchShortCustomer(string nationalID, int PortalID)
        {
            return SearchCustomer(nationalID, PortalID,true);
        }
        
       //core code esta en el primero
       public static CustomerDataWS.customerResponse SearchCustomer(string nationalID, int PortalID ,bool shortCustomer = true) {

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var cliente = new CustomerDataWS.CustomerDataWSClient();
                var ToSearch = new CustomerDataWS.customerRequest();


                ToSearch.customer = new CustomerDataWS.customerTo();
                ToSearch.customer.country = new CustomerDataWS.countryTo();
                ToSearch.customer.country.id = CountryID(PortalID);

                ToSearch.source = new CustomerDataWS.sourceTo();
                ToSearch.source.id = "WEB";

                ToSearch.customer.customerType = CustomerDataWS.customerType.N;
                ToSearch.customer.customerTypeSpecified = true;
                ToSearch.customer.nationalId = nationalID;
                var ToReturn = new CustomerDataWS.customerResponse(); 
               
                if(shortCustomer){
                    // este trae datos basicos
                     ToReturn = cliente.searchShortCustomer(ToSearch);
                }
                else{
                    // este trae el full info (muy pesado segun shiradit)
                     ToReturn = cliente.searchCustomer(ToSearch);
                }
                ///todo
                return ToReturn;
            }
            catch (Exception Ex)
            {
                throw new Exception("Hubo un error al tratar de Buscar a " + nationalID+ " con SearchCustomer. La excepcion dice: " + Ex.Message, Ex.InnerException);
            }

        }
       public static CustomerDataWS.customerResponse SearchCustomer(CustomerDataWS.customerTo TheCustomer, int PortalID,bool shortCustomer = true)
       {
           return SearchCustomer(TheCustomer.nationalId, PortalID ,shortCustomer);

       } 
         
       // estas son por una cuestion de simplificar y agilizar la programacion
       public static CustomerDataWS.customerResponse GetFullCustomer(string nationalID, int PortalID){
            var ToReturn = SearchCustomer(nationalID, PortalID, false);
            return ToReturn;
        }
       //public static CustomerDataWS.customerResponse GetFullCustomer(string nickname, string password)
        //{
        //    // este probablemente este mal hecho, pero por ahora sirve, no me gusta tener que loguear el customer para poder tener su data primero
        //    var TheCustomer = ValidateLogin(nickname, password);
        //    return GetFullCustomer(TheCustomer);

        //}
       public static CustomerDataWS.customerResponse GetFullCustomer(CustomerDataWS.customerTo TheCustomer, int PortalID)
        {
            var ToReturn = SearchCustomer(TheCustomer.nationalId, PortalID,  false);
            return ToReturn;

        }

       public static CustomerDataWS.customerResponse UpdateCustomer(CustomerDataWS.customerTo ToUpdate, int PortalID)
       {

            var ToReturn = new CustomerDataWS.customerResponse();
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var cliente = new CustomerDataWS.CustomerDataWSClient();

                var TheRequest = new CustomerDataWS.customerRequest();


                TheRequest.customer = ToUpdate;
             
                // este id debe venir del portal id
                TheRequest.customer.country.id = CountryID(PortalID);
                TheRequest.source = new CustomerDataWS.sourceTo();
                TheRequest.source.id = "WEB";
                TheRequest.customer.customerTypeSpecified = true;
                TheRequest.customer.customerType = CustomerDataWS.customerType.N;
                TheRequest.customer.operationType = CustomerDataWS.operationType.UPDATE;
                TheRequest.customer.operationTypeSpecified = true;
      
                // we update the customer
                ToReturn = cliente.updateCustomer(TheRequest);
               
           
            }
            catch {
               // llena el catch!
            }
            return ToReturn;
    }

       
        public static CustomerDataWS.customerTo EmptyCustomer(int PortalID) {

            //we generate the connection to the webservice client (ssl)
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new CustomerDataWS.CustomerDataWSClient();
            

            var ToReturn = new CustomerDataWS.customerTo();
            ToReturn.country = new CustomerDataWS.countryTo();
            // este id debe venir del portal id
            ToReturn.country.id = CountryID(PortalID);
           
            ToReturn.customerTypeSpecified = true;            
            ToReturn.customerType = CustomerDataWS.customerType.N;

            return ToReturn;
        
        }

        public static string CountryID(int PortalID){
            string ToReturn = "VE";

            if (PortalID == 0)
            {
                ToReturn = "VE";
            }
            else if (PortalID == 1)
            {
                ToReturn = "CO";
            }
            else {
                ToReturn = "VE";
            }
            return ToReturn;

    }

        public static List<ListItem> GetMatrialStatus()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();
          

            var Response = cliente.getMaritalStatusList(Request);

            var MyList = Response.resultList;
            
            foreach(MasterDataWS.masterData Data in MyList){
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetState(int PortalID)
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();
             var Country = new MasterDataWS.countryTo();
             // portal ID
             Country.id = CountryID(PortalID);
             Request.country = Country;

            var StateTo = new MasterDataWS.stateTo();


            var Response = cliente.getStatesByCountry(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }
        
        public static List<ListItem> GetCity(string StateID)
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();
            TheState.id = StateID;


            var Response = cliente.getCitiesByState(Request, TheState);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }
        
        public static List<ListItem> GetCountries()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();
            


            var Response = cliente.getCountries(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetOccupations()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();


          

            var Response = cliente.getOccupations(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetCommunicationTopic()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();




            var Response = cliente.getCommunicationTopics(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetCommunicationFrequencies()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();




            var Response = cliente.getCommunicationFrequencies(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetCommunicationChannels()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();




            var Response = cliente.getCommunicationChannels(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }

        public static List<ListItem> GetAddressTypes()
        {

            List<ListItem> ToReturn = new List<ListItem>();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var cliente = new MasterDataWS.MasterDataWSClient();

            var Request = new MasterDataWS.generalRequest();

            MasterDataWS.stateTo TheState = new MasterDataWS.stateTo();




            var Response = cliente.getAddressTypes(Request);

            var MyList = Response.resultList;

            foreach (MasterDataWS.masterData Data in MyList)
            {
                ToReturn.Add(new ListItem(Data.description, Data.id));
            }
            return ToReturn;

        }


    }
}

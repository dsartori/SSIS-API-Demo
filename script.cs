#region Help:  Introduction to the Script Component
/* The Script Component allows you to perform virtually any operation that can be accomplished in
 * a .Net application within the context of an Integration Services data flow.
 *
 * Expand the other regions which have "Help" prefixes for examples of specific ways to use
 * Integration Services features within this script component. */
#endregion

#region Namespaces
using System;
using System.Data;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;

#endregion

/// <summary>
/// This is the class to which to add your code.  Do not change the name, attributes, or parent
/// of this class.
/// </summary>
[Microsoft.SqlServer.Dts.Pipeline.SSISScriptComponentEntryPointAttribute]
public class ScriptMain : UserComponent
{
    #region Help:  Using Integration Services variables and parameters
    /* To use a variable in this script, first ensure that the variable has been added to
     * either the list contained in the ReadOnlyVariables property or the list contained in
     * the ReadWriteVariables property of this script component, according to whether or not your
     * code needs to write into the variable.  To do so, save this script, close this instance of
     * Visual Studio, and update the ReadOnlyVariables and ReadWriteVariables properties in the
     * Script Transformation Editor window.
     * To use a parameter in this script, follow the same steps. Parameters are always read-only.
     *
     * Example of reading from a variable or parameter:
     *  DateTime startTime = Variables.MyStartTime;
     *
     * Example of writing to a variable:
     *  Variables.myStringVariable = "new value";
     */
    #endregion

    #region Help:  Using Integration Services Connnection Managers
    /* Some types of connection managers can be used in this script component.  See the help topic
     * "Working with Connection Managers Programatically" for details.
     *
     * To use a connection manager in this script, first ensure that the connection manager has
     * been added to either the list of connection managers on the Connection Managers page of the
     * script component editor.  To add the connection manager, save this script, close this instance of
     * Visual Studio, and add the Connection Manager to the list.
     *
     * If the component needs to hold a connection open while processing rows, override the
     * AcquireConnections and ReleaseConnections methods.
     * 
     * Example of using an ADO.Net connection manager to acquire a SqlConnection:
     *  object rawConnection = Connections.SalesDB.AcquireConnection(transaction);
     *  SqlConnection salesDBConn = (SqlConnection)rawConnection;
     *
     * Example of using a File connection manager to acquire a file path:
     *  object rawConnection = Connections.Prices_zip.AcquireConnection(transaction);
     *  string filePath = (string)rawConnection;
     *
     * Example of releasing a connection manager:
     *  Connections.SalesDB.ReleaseConnection(rawConnection);
     */
    #endregion

    #region Help:  Firing Integration Services Events
    /* This script component can fire events.
     *
     * Example of firing an error event:
     *  ComponentMetaData.FireError(10, "Process Values", "Bad value", "", 0, out cancel);
     *
     * Example of firing an information event:
     *  ComponentMetaData.FireInformation(10, "Process Values", "Processing has started", "", 0, fireAgain);
     *
     * Example of firing a warning event:
     *  ComponentMetaData.FireWarning(10, "Process Values", "No rows were received", "", 0);
     */
    #endregion

    /// <summary>
    /// This method is called once, before rows begin to be processed in the data flow.
    ///
    /// You can remove this method if you don't need to do anything here.
    /// </summary>
    public override void PreExecute()
    {
        base.PreExecute();
        /*
         * Add your code here
         */
    }

    /// <summary>
    /// This method is called after all the rows have passed through this component.
    ///
    /// You can delete this method if you don't need to do anything here.
    /// </summary>
    public override void PostExecute()
    {
        base.PostExecute();
        /*
         * Add your code here
         */
    }

    public override void CreateNewOutputRows()
    {
        /*
          Add rows by calling the AddRow method on the member variable named "<Output Name>Buffer".
          For example, call MyOutputBuffer.AddRow() if your output was named "MyOutput".
        */
        string startDate = Variables.StartDate;
        string wUrl = "https://www.bankofcanada.ca/valet/observations/FXMUSDCAD/json?start_date=" + startDate;

        try
        {
           RootObject exchRates = getExchRates(wUrl);

           foreach (Observation observation in exchRates.observations)
            {

                RateOutputBuffer.AddRow();
                RateOutputBuffer.ExchangeRate = observation.FXMUSDCAD.v;
                RateOutputBuffer.Date = observation.d;
            }


        }
        catch (Exception e)
        {
            bool fireAgain = true;
            IDTSComponentMetaData100 myMetaData;
            myMetaData = this.ComponentMetaData;

            myMetaData.FireInformation(0, "Web Services Script", "Error connecting to web service:" + e.Message.ToString(), string.Empty, 0, ref fireAgain);
            
        }


    }
    RootObject getExchRates(string wURL)
    {

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(wURL);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        RootObject exchangeRates = null;

        try
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string json = null;

                using (StreamReader reader = new StreamReader(responseStream))
                {
                    json = reader.ReadToEnd().Replace("\\", "");
                    reader.Close();
                }

                JavaScriptSerializer sr = new JavaScriptSerializer();
                exchangeRates = sr.Deserialize<RootObject>(json.Trim('"'));

                
            }
            else
            {
                bool fireAgain = true;
                IDTSComponentMetaData100 myMetaData;
                myMetaData = this.ComponentMetaData;

                myMetaData.FireInformation(0, "Web Services Script", "Error connecting to web service", string.Empty, 0, ref fireAgain);

            }
        }
        catch (Exception e)
        {
            bool fireAgain = true;
            IDTSComponentMetaData100 myMetaData;
            myMetaData = this.ComponentMetaData;

            myMetaData.FireInformation(0, "Web Services Script", "Error connecting to web service:" + e.Message.ToString(), string.Empty, 0, ref fireAgain);

        }
        return exchangeRates;
        

    }


}
public class Terms
{
    public string url { get; set; }
}

public class FXMUSDCAD
{
    public string label { get; set; }
    public string description { get; set; }
}

public class SeriesDetail
{
    public FXMUSDCAD FXMUSDCAD { get; set; }
}

public class FXMUSDCAD2
{
    public decimal v { get; set; }
}

public class Observation
{
    public string d { get; set; }
    public FXMUSDCAD2 FXMUSDCAD { get; set; }
}

public class RootObject
{
    public Terms terms { get; set; }
    public SeriesDetail seriesDetail { get; set; }
    public List<Observation> observations { get; set; }
}

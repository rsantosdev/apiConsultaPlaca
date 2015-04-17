using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.IO.Compression;   
namespace ConsultaVeiculos
{
    public class ConsultaPlacaVeiculo
    {
       int numero = 0;
        public ConsultaPlacaVeiculo() { }
        //Chave Secreta para uso do servico
        private String chave = "shienshenlhq";
       private CookieContainer cookies = new CookieContainer();
        //Remove acentos do xml no retorno do resultado
        private string RemoverAcentos(string texto)
        {
            string s = texto.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            for (int k = 0; k < s.Length; k++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(s[k]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[k]);
                }
            }
            return sb.ToString();
        }
        private string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }
        
       
        public XmlDocument ConsultarPlaca(string placa)
        {
            XmlDocument document = new XmlDocument();
            XmlDocument doc = new XmlDocument();
            try
            {
                
                   int nErros = 0;
               
                        Uri urlpost = new Uri("http://sinespcidadao.sinesp.gov.br/sinesp-cidadao/ConsultaPlacaNovo27032014");
                        HttpWebRequest httpPostConsulta = (HttpWebRequest)HttpWebRequest.Create(urlpost);
                        string key = chave;
                        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                        byte[] keyByte = encoding.GetBytes(key);
                        HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);
                        byte[] messageBytes = encoding.GetBytes(placa);

                        byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);

                        string hmac2 = ByteToString(hashmessage);
                        //Xml que vai para o servidor do sinesp cidadao
                        StringBuilder postConsultaComParametros = new StringBuilder();
                        postConsultaComParametros.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>");
                        postConsultaComParametros.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" >");
                        postConsultaComParametros.Append("<soap:Header>");
                        postConsultaComParametros.Append("<dispositivo>GT-S1312L</dispositivo>");
                        postConsultaComParametros.Append("<nomeSO>Android</nomeSO>");
                        postConsultaComParametros.Append("<versaoSO>4.1.4</versaoSO>");
                        postConsultaComParametros.Append("<versaoAplicativo>1.1.1</versaoAplicativo>");
                        postConsultaComParametros.Append("<aplicativo>SinespCidadao</aplicativo>");
                        postConsultaComParametros.Append("<ip>192.168.1.1</ip>");
                        postConsultaComParametros.Append("<token>" + hmac2.ToLower() + "</token>");
                        postConsultaComParametros.Append("<latitude>0</latitude>");
                        postConsultaComParametros.Append("<longitude>0</longitude>");
                        postConsultaComParametros.Append("</soap:Header>");
                        postConsultaComParametros.Append("<soap:Body>");
                        postConsultaComParametros.Append("<webs:getStatus xmlns:webs=\"http://soap.ws.placa.service.sinesp.serpro.gov.br/\">");
                        postConsultaComParametros.Append("<placa>" + placa + "</placa>");
                        postConsultaComParametros.Append("</webs:getStatus></soap:Body>");
                        postConsultaComParametros.Append("</soap:Envelope>");

                        byte[] buffer2 = Encoding.ASCII.GetBytes(postConsultaComParametros.ToString());

                        httpPostConsulta.CookieContainer = cookies;
                        httpPostConsulta.Timeout = 900000;
                        httpPostConsulta.ContentType = "text/xml;charset=UTF-8";
                        httpPostConsulta.Method = "POST";
                        httpPostConsulta.ContentLength = buffer2.Length;


                        Stream PostData = httpPostConsulta.GetRequestStream();
                        PostData.Write(buffer2, 0, buffer2.Length);
                        PostData.Close();




                        HttpWebResponse responsePost = (HttpWebResponse)httpPostConsulta.GetResponse();
                        Stream istreamPost = responsePost.GetResponseStream();
                        StreamReader strRespotaUrlConsultaNFe = new StreamReader(istreamPost, Encoding.Default);



                        

                        doc.LoadXml(RemoverAcentos(strRespotaUrlConsultaNFe.ReadToEnd()));




                        XmlElement elementos = doc.DocumentElement;

                       
                   
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return doc;
        }
    }
}

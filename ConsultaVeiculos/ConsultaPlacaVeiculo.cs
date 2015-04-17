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
        private String chave = "shienshenlhq";
       private CookieContainer cookies = new CookieContainer();
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
        
       
        private String retorno(string cod, string msg, string situacao, string placa, string marca, string modelo, string AnoModelo, string AnoFabricacao, string cor, string uf, string cidade, string chassi)
        {
            String result = String.Empty;
            MemoryStream stream = new MemoryStream(); // The writer closes this for us

            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {



                //writer.Formatting = Formatting.Indented;
                //writer.Indentation = 4;
                writer.WriteStartDocument();
                writer.WriteStartElement("returnConsultaPlaca");
                writer.WriteStartElement("resultado");
                writer.WriteElementString("codigo", cod);

                writer.WriteElementString("mensagem", msg);
                writer.WriteEndElement();

                writer.WriteStartElement("Veiculo");
                writer.WriteElementString("situacao", situacao);
                writer.WriteElementString("placa", placa);
                writer.WriteElementString("marca", marca);
                writer.WriteElementString("modelo", modelo);
                writer.WriteElementString("ano_modelo", AnoModelo);
                writer.WriteElementString("ano_fabricacao", AnoFabricacao);
                writer.WriteElementString("cor", cor);
                writer.WriteElementString("estado", uf);
                writer.WriteElementString("municipio", cidade);
                writer.WriteElementString("chassi", chassi);
                writer.WriteEndElement();

                //</soap:Envelope>
                writer.WriteEndDocument();
                writer.Flush();
                writer.Flush();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                stream.Seek(0, SeekOrigin.Begin);

                result += reader.ReadToEnd();


            }

            return result;
        }
        public XmlDocument ConsultarPlaca(string placa)
        {
            XmlDocument document = new XmlDocument();
              
            try
            {
                
                   int nErros = 0;
               
                        Uri urlpost = new Uri("http://sinespcidadao.sinesp.gov.br/sinesp-cidadao/ConsultaPlacaNovo27032014");
                        HttpWebRequest httpPostConsultaNFe = (HttpWebRequest)HttpWebRequest.Create(urlpost);
                        string key = chave;
                        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                        byte[] keyByte = encoding.GetBytes(key);
                        HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);
                        byte[] messageBytes = encoding.GetBytes(placa);

                        byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);

                        string hmac2 = ByteToString(hashmessage);

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

                        httpPostConsultaNFe.CookieContainer = cookies;
                        httpPostConsultaNFe.Timeout = 900000;
                        httpPostConsultaNFe.ContentType = "text/xml;charset=UTF-8";
                        httpPostConsultaNFe.Method = "POST";
                        httpPostConsultaNFe.ContentLength = buffer2.Length;


                        Stream PostData = httpPostConsultaNFe.GetRequestStream();
                        PostData.Write(buffer2, 0, buffer2.Length);
                        PostData.Close();




                        HttpWebResponse responsePost = (HttpWebResponse)httpPostConsultaNFe.GetResponse();
                        Stream istreamPost = responsePost.GetResponseStream();
                        StreamReader strRespotaUrlConsultaNFe = new StreamReader(istreamPost, Encoding.Default);



                        XmlDocument doc = new XmlDocument();

                        doc.LoadXml(RemoverAcentos(strRespotaUrlConsultaNFe.ReadToEnd()));




                        XmlElement elementos = doc.DocumentElement;

                        #region "Xml Response"
                        string situacao = string.Empty;
                        string Marca = string.Empty;
                        string Modelo = string.Empty;
                        string Cor = string.Empty;
                        string AnoFabricacao = string.Empty;
                        string AnoModelo = string.Empty;
                        string Estado = string.Empty;
                        string Municipio = string.Empty;
                        string Chassi = string.Empty;
                        string Retorno = string.Empty;
                        foreach (XmlNode nodes in elementos.ChildNodes)
                        {
                            foreach (XmlNode nodes_1 in nodes.ChildNodes)
                            {
                                foreach (XmlNode nodes_2 in nodes_1.ChildNodes)
                                {
                                    foreach (XmlNode nodes_3 in nodes_2.ChildNodes)
                                    {
                                        switch (nodes_3.Name.ToUpper())
                                        {
                                            case "CODIGOSITUACAO":
                                                nErros = int.Parse(nodes_3.InnerText);
                                                break;
                                            case "MENSAGEMRETORNO":
                                                Retorno = nodes_3.InnerText;
                                                break;
                                            case "SITUACAO":
                                                situacao = RemoverAcentos(nodes_3.InnerText.ToUpper());
                                                break;
                                            case "MODELO":
                                                string[] MarcaModelo = nodes_3.InnerText.ToUpper().Split('/');
                                                Marca = MarcaModelo[0].ToString() == "I" ? MarcaModelo[1].Split(' ')[0].ToString() : MarcaModelo[0].ToString();
                                                Modelo = MarcaModelo[1].ToString();
                                                break;
                                            case "COR":
                                                Cor = nodes_3.InnerText.ToUpper();
                                                break;
                                            case "ANO":
                                                AnoFabricacao = nodes_3.InnerText;
                                                break;
                                            case "ANOMODELO":
                                                AnoModelo = nodes_3.InnerText;
                                                break;
                                            case "UF":
                                                Estado = nodes_3.InnerText.ToUpper();
                                                break;
                                            case "MUNICIPIO":
                                                Municipio = nodes_3.InnerText.ToUpper();
                                                break;
                                            case "CHASSI":
                                                Chassi = nodes_3.InnerText;
                                                break;
                                        }
                                    }

                                }

                            }

                        }
                        #endregion



                        document.LoadXml(retorno(nErros.ToString(), Retorno, situacao, placa, Marca, Modelo, AnoModelo, AnoFabricacao, Cor, Estado, Municipio, Chassi));

                   
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return document;
        }
    }
}

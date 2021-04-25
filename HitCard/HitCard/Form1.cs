using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace HitCard
{
    public partial class Form1 : Form
    {
        string path = @"..\..\lastmember.txt";//生成txt文件存储一个旧用户
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 300000;//毫秒为单位,设置每五分钟检查一次是否需要打卡,启动时不执行，等一个间隔再执行第一次
            if (File.Exists(path))//找出旧用户邮箱
            {
                StreamReader sr = new StreamReader(path, Encoding.Default);
                string line;
                char[] ss = { ',' };
                if ((line = sr.ReadLine()) != null)//判断文件是否为空
                {
                    string[] arr = line.ToString().Split(ss);
                    if (arr.Length == 9)
                    {
                        username = arr[0].Trim();                       
                        label7.Text = username;
                    }
                    else
                        label7.Text = "none";
                }
                else//文件为空
                    label7.Text = "none";
                sr.Close();
            }
            else//文件不存在
                label7.Text = "none";


        }
        
        bool flag1 = false;//判断是否打卡成功
        int x=1 ;//计数，用于显示一个时段内最终没打上卡的记录
        string username,password, fieldSQxq_Name, fieldSQgyl_Name, fieldSQqsh, fieldSQnj, fieldSQnj_Name, fieldSQbj, fieldSQbj_Name;//对应网页

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            skinEngine1.SkinFile = Application.StartupPath + @"\Wave.ssk";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string s;
            if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 12 && flag1 == false)//7-12,避开页面开启延后
                s = "早";
            else if (DateTime.Now.Hour > 21 && DateTime.Now.Hour < 24 && flag1 == false)//22-24,错开第一个小时高峰
                s = "晚";
            else 
                s = "无";   //打卡成功或者不在时间段   
            if (s!="无")//处于打卡状态,x=0
            {
                x = 0;
                if (flag1 == false)//在打卡时间段内却未打卡或打卡失败，则打卡
                {
                    try
                    {
                        if (login() == 0)
                        {
                            textBox5.Text = (s+"打卡成功" + "\r\n" + DateTime.Now.ToString());
                            flag1 = true;//该时间段已经打卡成功                          
                        }
                    }
                    catch (Exception ex)
                    {
                        //在时间段内某次失败
                        textBox5.Text = ("本次"+s+"打卡失败,失败原因：\r\n" + ex.Message + "\r\n" + DateTime.Now.ToString());
                    }

                }
                if (flag1 == true)
                {
                    DateTime current = DateTime.Now;
                    while (current.AddMilliseconds(60000) > DateTime.Now)//1分钟
                    {
                        Application.DoEvents();
                    }
            
                }
            }
            else if((DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 6)|| (DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 21))//在非打卡时间段内
            {
                x++;
                if (x==1&&flag1==false)//第一次进来
                    textBox6.Text = (textBox6.Text+  "打卡失败\t" + DateTime.Now.Year.ToString()+"/"+ DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString()+"\r\n");//在时间段内彻底失败
                flag1 = false;
                textBox5.Text = ("提示：不在打卡时间\t"+ DateTime.Now.ToString());
                
                
            } 
           
        }
       
        private void button1_Click(object sender, EventArgs e)//新用户启动，不能忽视表单内容
        {
            if (textBox1.Text == string.Empty)
                textBox5.Text = "失败，邮箱不能为空";
            else if (textBox2.Text == string.Empty)
                textBox5.Text = "失败，密码不能为空";
            else if (textBox7.Text == string.Empty)
                textBox5.Text = "失败，校区不能为空";
            else if (textBox8.Text == string.Empty)
                textBox5.Text = "失败，寝室楼不能为空";
            else if (textBox3.Text == string.Empty)
                textBox5.Text = "失败，寝室号不能为空";
            else if (textBox4.Text == string.Empty)
                textBox5.Text = "失败，班级不能为空";
            else
            {
                username = textBox1.Text.Trim();
                password = textBox2.Text.Trim();
                fieldSQxq_Name = textBox7.Text.Trim();
                fieldSQgyl_Name = textBox8.Text.Trim();
                fieldSQqsh = textBox3.Text.Trim();//315
                fieldSQnj = textBox1.Text.Substring(textBox1.Text.Length - 4, 4);//2118
                fieldSQnj_Name = "20" + textBox1.Text.Substring(textBox1.Text.Length - 2, 2);//2018
                fieldSQbj = (880 + Convert.ToInt32(textBox4.Text) - 26).ToString();//882
                fieldSQbj_Name = textBox1.Text.Substring(textBox1.Text.Length - 4, 4) + textBox4.Text.Trim();//211828

                label7.Text = username;
                timer1.Start();//启动
            }

        }
        private int login()
        {
            try
            {
                HttpClient httpClient = new HttpClient();//发送和接收http响应，自己管理cookie，只要把信息post过去，再get主页的时候就会得到登陆成功后的页面
                httpClient.MaxResponseContentBufferSize = 256000;//默认2G，取消该代码没有问题，网上有人设置为这个
                httpClient.DefaultRequestHeaders.Add("Referer", "https://ehall.jlu.edu.cn/");//获取与每个请求一起发送的标题，将请求的来源地址添加到头部
                //账户密码认证
                string LoginURL = "https://ehall.jlu.edu.cn/jlu_portal/login";//吉林大学网上办事大厅，即登录页面
                HttpResponseMessage response = httpClient.GetAsync(new Uri(LoginURL)).Result;//http响应消息,包含http内容
              
                string LoginInURL = "https://ehall.jlu.edu.cn/sso/login";//统一身份认证，即登陆后进入的页面
                var postcontent = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                };//要post的数据的list集合
                response = httpClient.PostAsync(new Uri(LoginInURL), new FormUrlEncodedContent(postcontent)).Result;// 登录提交
                //string LoginInResult = response.Content.ReadAsStringAsync().Result;
                //textBox6.Text = LoginInResult;

                //请求打卡网址，获取跨网站请求伪造Token，不固定
                string RequestURL = "https://ehall.jlu.edu.cn/infoplus/form/BKSMRDK/start";//打卡网址
                response = httpClient.GetAsync(new Uri(RequestURL)).Result;
                string RequestResult = response.Content.ReadAsStringAsync().Result;
                string csrfToken = new Regex("(?<=itemscope=\"csrfToken\" content=\").{1,200}(?=(\"))").Match(RequestResult).Groups[0].Value;
                //textBox6.Text = csrfToken;

                //判断是否可打卡并获得sessioid,不固定，post数据到接口网址
                string StartURL = "https://ehall.jlu.edu.cn/infoplus/interface/start";
                postcontent = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("idc", "BKSMRDK"),//域名
                    new KeyValuePair<string, string>("csrfToken", csrfToken)
                };
                response = httpClient.PostAsync(new Uri(StartURL), new FormUrlEncodedContent(postcontent)).Result;
                string StartResult = response.Content.ReadAsStringAsync().Result;
                //textBox6.Text = StartResult;{"errno":0,"ecode":"SUCCEED","error":"Succeed.","entities":["https://ehall.jlu.edu.cn/infoplus/form/41045744/render"]}
                string err = new Regex("(?<=errno\":).{1,10}(?=,)").Match(StartResult).Groups[0].Value;
                if (err == "22001")
                {
                    throw new Exception("提示：不在打卡时间");
                }
                string sessionid = new Regex("(?<=form/)\\d*(?=/render)").Match(StartResult).Groups[0].Value;
                //textBox6.Text =("StepID:" + sessionid);

                //请求文件路径render
                string RenderURL = "https://ehall.jlu.edu.cn/infoplus/interface/render";
                postcontent = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("stepId", sessionid),
                    new KeyValuePair<string, string>("csrfToken", csrfToken)
                };
                response = httpClient.PostAsync(new Uri(RenderURL), new FormUrlEncodedContent(postcontent)).Result;
                string RenderResult = response.Content.ReadAsStringAsync().Result;//得到json格式的结果
                //textBox6.Text = RenderResult;//{"errno":0,"ecode":"SUCCEED","error":"Succeed.","entities":[{"userId":"aef50926-c24d-11e9-87f9-0050568574b8",...},...

                var dict = JsonConvert.DeserializeObject<Dictionary<object, object>>(RenderResult);
                var arr = dict["entities"] as JArray;//获得报文中实体entities
                var entity = JsonConvert.DeserializeObject<Dictionary<object, object>>(arr[0].ToString());

                //
                var formDataDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(entity["data"].ToString());//要提交的数据从json转为net
                var boundFieldsDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(entity["fields"].ToString());

                var jss = new JavaScriptSerializer();//无类型

                Filldata(ref formDataDict);//以下是对从data字段中取出的内容中部分表格内容进行填写
                string formData = jss.Serialize(formDataDict);//对象转化为JSON字符串
                string boundFields = string.Join(",", boundFieldsDict.Keys.ToArray());//键值逗号分隔

                postcontent = new List<KeyValuePair<string, string>>();
                postcontent.Add(new KeyValuePair<string, string>("actionId", "1"));
                postcontent.Add(new KeyValuePair<string, string>("formData", formData));
                //postcontent.Add(new KeyValuePair<string, string>("nextUsers", "{}"));
                postcontent.Add(new KeyValuePair<string, string>("stepId", sessionid));
                DateTime d = new DateTime(1970, 1, 1, 8, 0, 0);
                string timeStamp = Convert.ToInt64((DateTime.Now - d).TotalSeconds).ToString();
                postcontent.Add(new KeyValuePair<string, string>("timestamp", timeStamp));
                postcontent.Add(new KeyValuePair<string, string>("boundFields", boundFields));
                postcontent.Add(new KeyValuePair<string, string>("csrfToken", csrfToken));

                string actionURL = "https://ehall.jlu.edu.cn/infoplus/interface/doAction";
                response = httpClient.PostAsync(new Uri(actionURL), new FormUrlEncodedContent(postcontent)).Result;
                string SubmitResult = response.Content.ReadAsStringAsync().Result;
                //根据返回内容判断是否成功
                var SubmitResultDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(SubmitResult);
                if (SubmitResultDict["ecode"].ToString() != "SUCCEED")
                {
                    throw new Exception("打卡失败");
                }
            }
            catch(Exception ex)//异常错误或者其他可返回ecode的错误
            {
                throw ex;
            }
           
               
            return 0;

        }

        //以下是对从data字段中取出的内容中部分表格内容进行填写，可扩充
        private void Filldata(ref Dictionary<object, object> formDataDict)
        {
            if (formDataDict["fieldXY1"].ToString() == "1")
            {
                formDataDict["fieldZhongtw"] = "1";
            }
            if (formDataDict["fieldXY2"].ToString() == "1")
            {
                formDataDict["fieldWantw"] = "1";
            }
            
            formDataDict["fieldZtw"] = "1";//体温1表示正常，2异常
            formDataDict["fieldSQxq_Name"] = fieldSQxq_Name;//校区
            if(fieldSQxq_Name== "中心校区") formDataDict["fieldSQxq"] = "1";
            else if (fieldSQxq_Name == "南岭校区") formDataDict["fieldSQxq"] = "2";
            else if (fieldSQxq_Name == "新平校区") formDataDict["fieldSQxq"] = "3";
            else if (fieldSQxq_Name == "南湖校区") formDataDict["fieldSQxq"] = "4";
            else if (fieldSQxq_Name == "和平校区") formDataDict["fieldSQxq"] = "5";
            else if (fieldSQxq_Name == "朝阳校区") formDataDict["fieldSQxq"] = "6";
            else if (fieldSQxq_Name == "前卫北区校区") formDataDict["fieldSQxq"] = "7";
            formDataDict["fieldSQgyl_Name"] = fieldSQgyl_Name;//公寓楼选择，南三5，北一1
            if (fieldSQxq_Name == "南苑3公寓") formDataDict["fieldSQgyl"] = "5";
            else if (fieldSQxq_Name == "北苑1公寓") formDataDict["fieldSQgyl"] = "1";
            formDataDict["fieldSQqsh"] = fieldSQqsh;//寝室号
            formDataDict["fieldSQnj"] = fieldSQnj;//2118
            formDataDict["fieldSQnj_Name"] = fieldSQnj_Name;//2018年级，17，18，19，20
            formDataDict["fieldSQbj_Name"] = fieldSQbj_Name;//211828班级，16，21-37
        }


        private void button2_Click(object sender, EventArgs e)//停止
        {
            timer1.Stop();
            flag1 = false;
        }
        
        private void button3_Click(object sender, EventArgs e)//保存当前用户信息进入文件，即只能保存一个用户信息
        {
            if (textBox1.Text == string.Empty)
                textBox5.Text = "失败，邮箱不能为空";
            else if (textBox2.Text == string.Empty)
                textBox5.Text = "失败，密码不能为空";
            else if (textBox7.Text == string.Empty)
                textBox5.Text = "失败，校区不能为空";
            else if (textBox8.Text == string.Empty)
                textBox5.Text = "失败，寝室楼不能为空";
            else if (textBox3.Text == string.Empty)
                textBox5.Text = "失败，寝室号不能为空";
            else if (textBox4.Text == string.Empty)
                textBox5.Text = "失败，班级不能为空";
            else
            {
                username = textBox1.Text.Trim();
                password = textBox2.Text.Trim();
                fieldSQxq_Name = textBox7.Text.Trim();
                fieldSQgyl_Name = textBox8.Text.Trim();
                fieldSQqsh = textBox3.Text.Trim();//315
                fieldSQnj = textBox1.Text.Substring(textBox1.Text.Length - 4, 4);//2118
                fieldSQnj_Name = "20" + textBox1.Text.Substring(textBox1.Text.Length - 2, 2);//2018
                fieldSQbj = (880 + Convert.ToInt32(textBox4.Text) - 26).ToString();//882
                fieldSQbj_Name = textBox1.Text.Substring(textBox1.Text.Length - 4, 4) + textBox4.Text.Trim();//211828

                FileStream fs = new FileStream(path, FileMode.Create);//没有则创建，有则覆盖
                //获得字节数组
                string s = username + "," + password + "," + fieldSQqsh + "," + fieldSQnj + "," + fieldSQnj_Name + "," + fieldSQbj + "," + fieldSQbj_Name + "," + fieldSQxq_Name + "," + fieldSQgyl_Name;
                byte[] data = Encoding.Default.GetBytes(s);
                //开始写入
                fs.Write(data, 0, data.Length);
                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
                textBox5.Text = "新用户信息保存完毕";
                label7.Text = username;
            }
        }

        private void button4_Click(object sender, EventArgs e)//旧用户启动
        {
            //读取文件，找到上一个用户信息，忽略所有表单上的填写
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path, Encoding.Default);
                string line;
                char[] ss = { ',' };
                if ((line = sr.ReadLine()) != null)//判断文件是否为空
                {
                    string[] arr = line.ToString().Split(ss);
                    if (arr.Length == 9)
                    {
                        username = arr[0].Trim();
                        password = arr[1].Trim();
                        fieldSQqsh = arr[2].Trim();
                        fieldSQnj = arr[3].Trim();
                        fieldSQnj_Name = arr[4].Trim();
                        fieldSQbj = arr[5].Trim();
                        fieldSQbj_Name = arr[6].Trim();
                        fieldSQxq_Name = arr[7].Trim();
                        fieldSQgyl_Name = arr[8].Trim(); 
                        textBox5.Text = "旧用户信息加载完毕";
                        label7.Text = username;

                        timer1.Start();//启动
                    }
                    else
                    {
                        textBox5.Text = "失败，保存的旧用户信息不全，只能进行新用户启动";
                    }
                }
                else//文件为空
                {
                    textBox5.Text = "失败，文件中没有保存的用户，只能进行新用户启动";
                }
                sr.Close();
            }
            else//文件不存在
                textBox5.Text = "失败，没有保存旧用户的文件，只能进行新用户启动";
        }
    }
}

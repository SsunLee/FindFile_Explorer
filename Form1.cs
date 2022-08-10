using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace FindFile_Sun
{


    public partial class Form1 : Form 
    {
        static FindClass fc = new FindClass();

        //private List<Bitmap> list = new List<Bitmap>();
        public Form1()
        {
            InitializeComponent();

            Icon = Properties.Resources.computer_laptop_notebook_screen_technology_icon_0;
             radAll.Checked = true;
         }

        #region 검색어 입력하려고 할 하면 예시지워주기
        private void ClearExample(object sender, EventArgs e)
        {
            string strKey = txtKeyword.Text;

           if (strKey.IndexOf("ex)") > 0 )
            {
                Console.WriteLine("포함 됨");
            }
        }
    
        #endregion

        #region 처음 폼 로드 될 때 
       // public DispatcherTimer Timer = new DispatcherTimer();   // WindowBase.dll
        public Random Ran = new Random();
        private void Form1_Load(object sender, EventArgs e)
        {
            //Timer.Interval = TimeSpan.FromSeconds(3);
            //Timer.Tick += new EventHandler(SlideShow_Image);
            //Timer.Start();

            listView1.View = View.Details;
            listView1.CheckBoxes = true;
            listView1.GridLines = true;
            listView1.MultiSelect = true;

            listView1.Columns.Add(@"파일명",150);
            listView1.Columns.Add(@"크기", 100);
            listView1.Columns.Add(@"확장자");
            listView1.Columns.Add(@"최근수정날짜");
            listView1.Columns.Add(@"경로");

            // 예시 표현 및 폰트 컬러 설정 
            txtKeyword.Text = @"ex) *연말정산*";
            txtKeyword.Font = new Font("맑은 고딕", 9, FontStyle.Italic);
            txtKeyword.ForeColor = Color.Gray;
        }
        #endregion

        #region 파일선택창
        private void SetPath(object sender, EventArgs e)
        {
            // 외부 api 사용함
            var dir = new CommonOpenFileDialog();       // 선언 var는 좀 더 공부해야 함

            dir.IsFolderPicker = true;  // 폴더선택 허용

            if (dir.ShowDialog() == CommonFileDialogResult.Ok)      // 폴더만 선택 한다면
            {
                System.Diagnostics.Debug.Print("폴더만 눌렀음");
                System.Diagnostics.Debug.Print(dir.FileName);
                txtPath.Text = dir.FileName;    // 선택한 path 넣어줌
            }
            else // 취소하거나 esc했을 경우?
            {
                return;
            }
            txtKeyword.Focus();
        }

        #endregion

        #region Enter Key 했을 때
        private void EnterClick(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(13))
            {
                SearchButton(sender, e);
            }
        }
        #endregion

        #region 파일 찾기 기능
        private void SearchButton(object sender, EventArgs e)
        {
            if (txtKeyword.Text.Trim().Equals(""))  // 입력 된 검색어가 없다면
            {
                MessageBox.Show("검색어를 입력해주세요.", "lee.sunbae", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (txtPath.Text.Trim().Equals(""))
            {
                MessageBox.Show("경로를 지정해주세요.", "lee.sunbae", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            listView1.BeginUpdate();

            FindClass fc = new FindClass(txtPath.Text, txtKeyword.Text);

            Thread th1 = new Thread(new ThreadStart(fc.GoFind));
            th1.Start();
            th1.Join(); // 다른 스레드 호출 방지

            IEnumerable<FileInfo> files = fc.getResult();

            pushDel pd = new pushDel(pushData);
                       
            Thread th2 = new Thread((delegate () { pushData(files); }));

 
            th2.Start();

            //스레드가모두멈추기까지대기
            //Thread.Sleep(100);

            listView1.EndUpdate();
        }

        delegate void pushDel(IEnumerable<FileInfo> files);

        // 넣기
         void pushData(IEnumerable<FileInfo> files)
        {

            foreach (FileInfo file in files)
            {
                ListViewItem item = new ListViewItem(file.Name, 0);       // 파일 명
                long aSize = Convert.ToInt64(file.Length.ToString());
                item.SubItems.Add(FormatBytes(aSize));                    // 파일 크기
                item.SubItems.Add(file.Extension.ToString());                // 파일 종류 (확장자)
                item.SubItems.Add(file.LastAccessTime.ToString());        // 파일 마지막 수정날짜
                item.SubItems.Add(file.FullName.ToString());                  // 경로

                if (listView1.InvokeRequired == true)
                {  // 익명 메소드를 이용하여 invoke 처리 
                    listView1.Invoke(new MethodInvoker(delegate () { listView1.Items.Add(item); }));
                }
                else
                {
                    listView1.Items.Add(item);
                }

            }

        }

        private void FindFiles(string dir, string keyword)
        {
            // 경로 저장
            //string[] files = System.IO.Directory.GetFiles(dir, keyword,SearchOption.AllDirectories);
            DirectoryInfo di = new DirectoryInfo(dir);
            //FileInfo[] files = di.GetFiles(keyword, SearchOption.AllDirectories);

            // 디렉토리 IEnumerable 사용하여 검색
            IEnumerable<FileInfo> files = di.GetFiles(keyword, SearchOption.AllDirectories);

            // IEnumerable을 이용하면 Fileinfo의 length 가 없기 때문에, 아래와 같이 구함.
            int c = files.Count(x => !x.Attributes.HasFlag(FileAttributes.ReparsePoint));


            // 결과 표시
            if (c > 0)   // 검색 결과가 있다면
            {
                laResult.Text = "검색 결과 : " + c + " 건이 검색되었습니다.";
                listView1.Items.Clear();
            }
            else
            {
                laResult.Text = "검색 결과 : " + " 검색 결과가 없습니다.";
                listView1.Items.Clear();
            }

            // 구조 변경 해야 함
            // 현 : 반복하여 List View에 바로 넣음
            // 후 : ListOf() 에 담아서 한번에 Binding 할 예정

            foreach (FileInfo file in files)
            {
                ListViewItem item = new ListViewItem(file.Name, 0);       // 파일 명
                long aSize = Convert.ToInt64(file.Length.ToString());
                item.SubItems.Add(FormatBytes(aSize));                    // 파일 크기
                item.SubItems.Add(file.Extension.ToString());                // 파일 종류 (확장자)
                item.SubItems.Add(file.LastAccessTime.ToString());        // 파일 마지막 수정날짜
                item.SubItems.Add(file.FullName.ToString());                  // 경로

                listView1.Items.Add(item);
            }

        }

        #endregion

        #region 아이템 더블클릭 시 경로 열어주기

        private void DbClickItem(object sender, EventArgs e)
        {
            string a = listView1.SelectedItems[0].SubItems[4].Text;

            System.Diagnostics.Process.Start(a);
        }


        #endregion

        #region 바이트 변환 함수
        public string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }
        #endregion

        #region 이미지슬라이드쇼
        
        private void SlideShow_Image(object sender, EventArgs e)
        {
            Random r = new Random();
            int _index = r.Next(1, 4);
            //pictureBox1.Image = new Bitmap(list[_index]);

            byte[] image;






        }

        #endregion


    }
    class FindClass
    {
        private string path;
        private string key;
        private IEnumerable<FileInfo> files;

        public IEnumerable<FileInfo> getResult() { return this.files; }

        public FindClass() { }

        public FindClass(string path, string key)   // file info
        {
            this.path = path; this.key = key;
        }

        public void GoFind()    // 찾기
        {
            lock (this)
            {
                // 경로
                DirectoryInfo di = new DirectoryInfo(path);
                //FileInfo[] files = di.GetFiles(keyword, SearchOption.AllDirectories);

                // 디렉토리 IEnumerable 사용하여 검색
                IEnumerable<FileInfo> files = di.GetFiles(key, SearchOption.AllDirectories);

                // IEnumerable을 이용하면 Fileinfo의 length 가 없기 때문에, 아래와 같이 구함.
                int c = files.Count(x => !x.Attributes.HasFlag(FileAttributes.ReparsePoint));

                // 결과 표시
                if (c > 0)   // 검색 결과가 있다면
                {
                    Console.WriteLine("검색 결과 : " + c + " 건이 검색되었습니다.");
                }
                else
                {
                    Console.WriteLine("검색 결과 : " + " 검색 결과가 없습니다.");
                }

                // 여기서 저장
                this.files = files;

            }

        }

    }
}

FindFile_Sun
---
### 실행화면
![image](https://user-images.githubusercontent.com/41108401/183918309-88ae0239-298d-497b-a9b0-786a8d8f41c6.png)

<br>

![aa](https://user-images.githubusercontent.com/41108401/183920945-5a7ea759-8a38-4b84-a166-004c523b2d9e.gif)

<br>



### Code

```cs
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

```

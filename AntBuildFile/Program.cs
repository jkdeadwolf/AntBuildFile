using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace AntBuildFile
{
    class Program
    {
        const string DEFAULT_BASEDIR = ".";
        const string DEFAULT_OUTPUT = "output";
        const string DEFAULT_SRC = "src";

        static void Main(string[] args)
        {
            //{1}  lib 目录
            //{2}  src 目录
            
            //{0}  项目名
            //{1}  basedir 
            //{2}  输出目录

            string buildXml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
                <project name=""{0}"" default=""compile"" basedir=""{1}"">
	                <!-- 创建编译文件存放父目录  -->
	                <target name=""init"" description=""创建编译文件存放父目录 {2}"">
		                <mkdir dir=""{2}""/>
	                </target>

                    {4}
                        
	                <target name=""compile"" depends=""init"" description=""编译"">
		                <javac destdir=""{2}"" includeantruntime=""on"" fork=""yes"" encoding=""UTF-8"" debug=""true"" debuglevel=""vars,lines,source""  >
			                <compilerarg value=""-Xlint:unchecked""/>
			                <compilerarg value=""-Xlint:deprecation""/>
			                <compilerarg value=""-XDignore.symbol.file""/>
                            {3}
			                <classpath refid=""lib-classpath""/>
		                </javac>
	                </target> 
	
	                <target name=""clean"" depends=""compile"" description=""清理临时文件"">
		                <delete dir=""{2}""/>
	                </target>

                </project>";



            string libPath = @"	<!-- 第三方jar包的路径 -->
            	            <path id=""lib-classpath"" description=""第三方jar包的路径"">
		                        {0}
	                        </path>";
//	            <path id=""lib-classpath"" description=""第三方jar包的路径"">
//		            <fileset dir=""{0}"">
//			            <include name=""**/*.jar""/>
//		            </fileset>
//	            </path>";



            string srcPath = @"	<!-- Src 的路径 -->
	            <path id=""src-path"" description=""Src 的路径"">
		            {0}
	            </path>";

            ////            <fileset dir=""{0}"">
            //            <include name=""**/*.java""/>
            //        </fileset>


            string buildFileName = args[0] + @"\build.xml";

            if (File.Exists(buildFileName) == true)
            {
                File.Copy(buildFileName, buildFileName + ".bak",true);
            }

            DirectoryInfo di = new DirectoryInfo(args[0]);
            string projectName = di.Name;

            string classpathFile = args[0] + @"\.classpath";
            string classpathFileText = string.Empty;
            try
            {
                classpathFileText = File.ReadAllText(classpathFile);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            MatchCollection srcMatchs = Regex.Matches(classpathFileText, @"classpathentry .*kind=""src"" path=""(?<SRC>.*)""", RegexOptions.IgnoreCase);
            StringBuilder srcSB = new StringBuilder();

            foreach(Match  match in srcMatchs)
            {
                srcSB.AppendLine(string.Format(@"<src path=""{0}"" />", match.Groups["SRC"].Value));
            }

            string srcPathText = srcSB.ToString();

            MatchCollection libMatchs = Regex.Matches(classpathFileText, @"classpathentry kind=""lib"" path=""(?<LIB>[^""]*)""", RegexOptions.IgnoreCase);
            StringBuilder libSB = new StringBuilder();
            Dictionary<string, string> dicLib = new Dictionary<string, string>();

            foreach (Match match in libMatchs)
            {
                
                string libFile = match.Groups["LIB"].Value;
                string libFilePath = libFile.Substring(0, libFile.LastIndexOf('/'));
                if (dicLib.ContainsKey(libFilePath) == false)
                {
                    dicLib.Add(libFilePath, libFilePath);
                    libSB.AppendLine(string.Format(@"<fileset dir=""{0}"" />", libFilePath));
                }
            }

            string libPathText = string.Format(libPath, libSB.ToString());

            Match outputMatch = Regex.Match(classpathFileText, @"classpathentry kind=""output"" path=""(?<OUTPUT>.*)""", RegexOptions.IgnoreCase);
            string outputPath = DEFAULT_OUTPUT;
            if (outputMatch.Success)
            {
                outputPath = outputMatch.Groups["OUTPUT"].Value;
            }

            string baseDir = DEFAULT_BASEDIR;

            string buildAll = string.Format(buildXml, projectName, baseDir, outputPath, srcPathText, libPathText);
            File.WriteAllText(buildFileName, buildAll, Encoding.UTF8);
        }
    }
}

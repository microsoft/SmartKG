# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.


# Usage

SmartKG是一款轻量级知识图谱可视化+智能对话框架。它能够根据用户输入的实体和关系数据自动生成知识图谱，并提供图谱可视化及基于图谱的智能对话机器人。

## 1. 下载、运行SmartKG

### 1.1 下载安装下列软件： 

	(1) git: https://gitforwindows.org/
	(2) Visual Studio 2019: https://visualstudio.microsoft.com/zh-hans/downloads/ 	
	(3) Node JS: https://nodejs.org/zh-cn/download/
	（4) Python 3

### 1.2 Clone Repositry

	(1) 进入 ${SourceCode_Base_Path}
	(2) 运行：
		git clone https://github.com/microsoft/SmartKG

### 1.3 运行SmartKG后端
	
	(1) 用Visual Studio 2019 打开 ${SourceCode_Base_Path}/SmartKG/src/SmartKG.sln
	(2) 修改${SourceCode_Base_Path}/SmartKG/src/SmartKG.KGBot/appsettings.json文件，将其中FileDataPath中的KGFilePath，NLUFilePath和VCFilePath的值分别换成${SourceCode_Base_Path}/SmartKG/src/SmartKG.DataProcessor/Resources/Physics2目录下的KG，NLU和Visualization目录。
	(3) 选中SmartKG.KGBot项目，运行“SmartKG.KGBot"。

### 1.4 运行SmartKG前端

	(1) 进入${SourceCode_Base_Path}/SmartKG/SmartKGUI/
	(2) 在首次运行SmartKGUI前运行：
		npm i
	(3) 运行：
		npm run serve

### 1.5 本机访问前端

	打开浏览器，输入地址：http://localhost:8080，
	可以（i）在输入框输入实体名进行查询；（ii）选中场景查看全场景图谱；（iii）点击右下角耳机图案，打开对话窗口进行对话。

## 2. 生成自己的图谱

### 2.1 填写模板

	(1) 模板位于 ${SourceCode_Base_Path}/SmartKG/src/SmartKG.DataProcessor/Resources/_Template/SmartKG_KGDesc_Template.xlsx
	(2) 模板分为两页：顶点页和边页。前者为图谱中的实体，后者为实体间的关系。根据模板样例填写你自己的顶点和边数据。
	(3) 将填写好的excel文件放在${SourceCode_Base_Path}/SmartKG/src/martKG.DataProcessor/Resources/_DataPreProcess/excel/目录下，可创建子目录

### 2.2 生成KG，NLU和VC文件

	(1) 仿照${SourceCode_Base_Path}/SmartKG/src/martKG.DataProcessor/Resources/_DataPreProcess/scripts/目录下的ProcessData${Scenario}.py 创建自己的数据处理py文件，放在相同目录下。
	(2) 用Python 3 运行自己生成数据处理脚本。
	(3) 目标目录下会生成三个子目录：KG，NLU和VC，KG和VC下生成json文件，NLU目录下为tsv文件。

### 2.3 修改appsetting.json

	修改${SourceCode_Base_Path}/SmartKG/src/SmartKG.KGBot/appsettings.json文件，将其中FileDataPath中的KGFilePath，NLUFilePath和VCFilePath的值分别换成2.2中生成的KG，NLU和VC目录的路径。

### 2.4 重新运行SmartKG后端

-----------------

# Development Branch Only

## 3. docker images

### 3.0 compile of smartkg image with Visual Studio

配置：Release
目标框架：netcoreapp2.1.16
目标运行时：可移植的

### 3.1 Download or clone the directory of SmartKG/dockers/smartkg_services/ to a machine on which docker and docker-compose were installed.

### 3.2 enter SmartKG/dockers/smartkg_services/ 

### 3.3 run following command to start smartkg backend  
	1) sudo docker-compose build --build-arg DOCKER_HOST=${docker_host_ip}
	2) sudo docker-compose up

### 3.4 access backend：http://${docker_host_ip}:8082/swagger/index.html to view all API of smartkg 

             access UI：http://${docker_host_ip}:8083 for visulization and chatbot
                               http://${docker_host_ip}:8083/upload for datastore management and data upload




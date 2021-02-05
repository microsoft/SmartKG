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

## 0. 更加详细的安装、编译和使用方法请见：https://github.com/microsoft/SmartKG/blob/master/SmartKG_Spec.pdf

## 1. 下载 SmartKG

### 1.1 下载安装下列软件： 

	(1) git: https://gitforwindows.org/
	(2) Visual Studio 2019: https://visualstudio.microsoft.com/zh-hans/downloads/ 	
	(3) Node.JS: https://nodejs.org/zh-cn/download/ (推荐 14.15.4)
	(4) Python 3 (推荐 3.7)

### 1.2 Clone Repositry

	(1) 进入 ${SourceCode_Base_Path}
	(2) 运行：
		git clone https://github.com/microsoft/SmartKG

### 1.3 目录结构

	在目录dockers里，是已经编译好前后端服务的二进制码、配置文件，以及对应的docker image。

	在目录Resources里，有用户上传数据的模板和用来做测试的图谱数据。其中，template子目录中是模板，用户如果要创建自己的知识图谱，就需要按照模板的格式要求，填入相应实体和实体关系。Input子目录内有包括西游记、红楼梦、中学物理课以及COVID19等数据。

	在目录SmartKGLocalBase里，是SmartKG后端服务会调用的一些Python文件和用于存储运行时数据的本地文件的目录。

	在目录SmartKGUI下面，是SmartKG UI的源代码。是基于Node.js, 用JavaScript开发的。

	在目录src下面，是SmartKG后端服务的源代码。这部分源代码是基于Asp.NET 框架，用C# 开发的。

## 2. 运行SmartKG

### 2.1 Windows 上运行 SmartKG

	
	(1) 在Windows环境里启动 SmartKG，首先应该新建一个目录，例如，我们创建一个名为 temp 的目录。然后，从本地 Repo 中dockers目录内的smartkg子目录中，将两个zip文件和local_config文件夹复制到temp文件夹中。还需要从dockers目录的ui 子目录中，将唯一一个zip文件和local_config文件夹复制到temp文件夹中。

	(2) 把temp目录中的三个压缩文件直接就地解压缩（Extraction Here）。

	(3) 并将temp/local_config中的config.js移动到解压缩后的 temp/smartkgui/public中。

	(4) 将local_confing的appsettings.File.json文件复制一份，并改名为appsettings.json，移动到temp/smartkg中。

	(5) 命令行进入 temp/smartkg，运行命令：dotnet SmartKG.KGBot.dll
	    此命令用于启动 SmartKG 后端。启动后，会生成一个Now listening on地址，我们直接访问地址就可以。在浏览器中输入地址（例如：http://localhost:5000），可以访问后，说明后台启动成功了。

	(6) 进入temp/smartkgui目录下，输入命令：
		npm i
	    这个命令只需要第一次使用时运行，如果已经运行过，就可以跳过了。

	    此命令运行成功后，再输入命令：
		npm run serve

	    运行成功后，会给一个访问地址，例如：http://localhost:8080/
            用这个地址就可以直接在浏览器中访问 SmartKG 的主界面了，这个地址加上 "/upload" 是 SmartKG 的上传页面。


### 2.2 Linux 上运行 SmartKG

	(0) 在Linux环境部署前，需要提前安装好 docker 和 docker-compose。

	(1) 打开 Repo 中的 dockers 目录，将里面的 smartkg_services 目录整体压缩，并拷贝到Linux机器上，一般放在用户目录下。

	(2) 在 Linux 系统进入用户目录，并解压缩 smartkg_services.zip。
	
	(3) 进入 smartkg_services/ 目录，运行下列命令启动 SmartKG docker-compose OneBox:

		1) sudo docker-compose build --build-arg DOCKER_HOST=${docker_host_ip}
		2) sudo docker-compose up

	     访问后端：http://${docker_host_ip}:8082/swagger/index.html 能够获得 API 列表

             访问前端：http://${docker_host_ip}:8083 主页面
                       http://${docker_host_ip}:8083/upload 上传页面

## 3. 生成自己的图谱

### 3.1 填写模板

	(1) 模板位于 ${SourceCode_Base_Path}/SmartKG/Resources/_Template/SmartKG_KGDesc_Template.xlsx
	(2) 模板分为两页：顶点页和边页。前者为图谱中的实体，后者为实体间的关系。根据模板样例填写你自己的顶点和边数据。

### 3.2 将填写好的 excel 文件通过 SmartKG 的前端 upload 页面上传

## 4. 编译 SmartKG

### 4.1 用 Visual Studio 2019 编译 SmartKG

	回到Repo目录，进入src文件夹，启动SmartKG.sln。
	进入到VS后，点击Build的Bulid Solution，编译源代码。

### 4.2 Publish SmartKG

	在 Visual Studio 中打开 SmartKG solution 后，右键点击 SmartKG.KGBot project，选择点击 Publish，接着点击 Folder，以及 Next。
	
	配置 Publish 选项：

		配置：Release
		目标框架：netcoreapp2.1.16
		目标运行时：可移植的

	保存设置后直接 Publish！




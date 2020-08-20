<template>
  <div class="main">
    <div class="container">
      <p class="title">请您选择数据仓库</p>
      <div id="download">
        <div class="list">
          <div v-for="item of dataStores" v-bind:key="item.key">
            <div class="list-item">
              <el-radio v-model="radio" :label="item.name" border style="margin-bottom:10px">
                {{item.name}}
                <i class="el-icon-s-flag" v-if="item.name==defaultDataStore"></i>
              </el-radio>
              <el-button type="primary" @click="openUploadwin(item.name)" style="height:40px;">上传数据</el-button>
            </div>
          </div>
        </div>
        <el-button type="primary" @click="dialogCreateVisible = true">新建数据仓库</el-button>
        <el-button type="primary" @click="setDefault()">设为默认仓库</el-button>
        <el-button type="primary" @click="dialogDelVisible = true">删除仓库</el-button>
        <el-dialog title="新建数据库" :visible.sync="dialogCreateVisible" width="30%">
          <p>请输入数据仓库的名称</p>
          <p>
            <el-input v-model="input" placeholder="请输入仓库名称" style="width:60%; margin-top:20px;"></el-input>
          </p>
          <span slot="footer" class="dialog-footer">
            <el-button @click="dialogCreateVisible = false">取 消</el-button>
            <el-button type="primary" @click="saveDataStore()">确 定</el-button>
          </span>
        </el-dialog>

        <el-dialog title="删除数据库" :visible.sync="dialogDelVisible" width="30%">
          <p>您确实要删除这个数据仓库吗？</p>
          <span slot="footer" class="dialog-footer">
            <el-button @click="dialogDelVisible = false">取 消</el-button>
            <el-button type="primary" @click="del()">确 定</el-button>
          </span>
        </el-dialog>

        <el-dialog title="上传数据" :visible.sync="dialogUploadVisible" width="30%">
          <div class="file-item" v-for="item of uploadFiles" v-bind:key="item.key">
            <el-input
              v-model="item.scenario"
              size="small"
              placeholder="请输入场景名称"
              style="width:40%; margin-right:30px;"
            ></el-input>
            <input class="file" type="file" @change="getFile($event, item)" />
          </div>
          <div>
            <i class="add-btn el-icon-circle-plus-outline" @click="addFile()"></i>
          </div>
          <span slot="footer" class="dialog-footer">
            <el-button @click="dialogUploadVisible = false">取 消</el-button>
            <el-button type="primary" @click="uploadSend()">确 定</el-button>
          </span>
        </el-dialog>
      </div>
    </div>
  </div>
</template>

<style type="text/css">
@import "../assets/upload.css";
</style>

<script>
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import axios from "axios";

export default {
  name: "Upload",
  data() {
    return {
      baseURL: window.urlapi,
      dataStores: [],
      dialogCreateVisible: false,
      dialogDefaultVisible: false,
      dialogDelVisible: false,
      dialogUploadVisible: false,
      input: "",
      radio: 1,
      uploadFiles: [],
      defaultDataStore: "",
    };
  },
  components: {},
  methods: {
    handleClose(done) {
      this.$confirm("确认关闭？")
        .then((_) => {
          done();
        })
        .catch((_) => {});
    },
    saveDataStore() {
      axios
        .post(`${this.baseURL}/api/DataStoreMgmt`, {
          datastoreName: this.input,
        })
        .then((res) => {
          this.input = "";
          this.getList();
          this.dialogCreateVisible = false;
        });
    },

    setDefault() {
        let uploadFormData = new FormData();
        uploadFormData.append("DatastoreName", this.radio);       
        let config = {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        };
      axios
        .post(`${this.baseURL}/api/PreProcess/reload`, uploadFormData, config)
        .then((res) => {
          this.getList();
        });
    },

    getList() {
      this.dataStores = [];
      axios.get(`${this.baseURL}/api/DataStoreMgmt`).then((res) => {
        for (let i = 0; i < res.data.datastoreNames.length; i++) {
          this.dataStores.push({
            name: res.data.datastoreNames[i],
          });
        }
        axios.get(`${this.baseURL}/api/DataStoreMgmt/current`).then((res1) => {
          this.defaultDataStore = res1.data.currentDatastoreName;
        });
      });
    },
    del() {
      console.log(this.radio, 3434);
      let config = {
        headers: {
          accept: "text/plain",
          "Content-Type": "application/json-patch+json",
        },
        data: { datastoreName: this.radio },
      };
      axios.delete(`${this.baseURL}/api/DataStoreMgmt`, config).then((res) => {
        this.getList();
        this.dialogDelVisible = false;
      });
    },

    getFile(event, item) {
      item.file = event.target.files[0];
    },
    openUploadwin(name) {
      this.currentDataStore = name;
      this.uploadFiles = [];
      this.uploadFiles.push({ scenario: "", file: "" });
      this.dialogUploadVisible = true;
    },
    addFile() {
      this.uploadFiles.push({ scenario: "", file: "" });
    },
    uploadSend() {
      let result = [];
      for (let i = 0; i < this.uploadFiles.length; i++) {
        let formData = new FormData();
        formData.append("DatastoreName", this.currentDataStore);
        formData.append("Scenario", this.uploadFiles[i].scenario);
        formData.append("UploadFile", this.uploadFiles[i].file);
        let config = {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        };
        axios
          .post(`${this.baseURL}/api/PreProcess/upload`, formData, config)
          .then((res) => {
            result.push(true);
            console.log(res);
          });
      }
      let timer = setInterval(() => {
        if (result.length == this.uploadFiles.length) {
          if (result.every((item) => item == true)) {
            alert("上传成功");
            this.dialogUploadVisible = false;
          } else {
            alert("上传失败");
          }
          clearInterval(timer);
        }
      }, 500);
    },
  },
  mounted() {
    this.getList();
  },
};
</script>


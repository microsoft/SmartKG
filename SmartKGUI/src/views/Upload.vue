<template>
  <div class="main">
    <div class="container">
      <p class="title">请您选择数据仓库</p>
      <div id="download">
        <div class="list">
          <!-- <div v-for="item of dataStores" v-bind:key="item.key">
            <div class="list-item">
              <el-radio
                v-model="radio"
                :label="item.name"
                border
                style="margin-bottom: 10px"
              >
                {{ item.name }}
                <i
                  class="el-icon-s-flag"
                  v-if="item.name == defaultDataStore"
                ></i>
              </el-radio>
              <el-button
                type="primary"
                @click="openUploadwin(item.name)"
                style="height: 40px"
                >上传数据</el-button
              >
            </div>
          </div> -->
          <el-select
            v-model="radio"
            placeholder="请选择数据库"
            :change="getDataStore()"
          >
            <el-option
              v-for="item in dataStores"
              :key="item.id"
              :label="item.name"
              :value="item.name"
            ></el-option>
          </el-select>
        </div>
        <el-button type="primary" @click="dialogCreateVisible = true"
          >新建数据仓库</el-button
        >
        <el-button type="primary" @click="openDel()">删除仓库</el-button>
        <el-button type="primary" @click="openUploadwin(radio)"
          >上传数据</el-button
        >
        <el-button type="primary" @click="reloadData()">加载数据</el-button>
        <el-button type="primary" @click="colorVisible = true"
          >设置颜色</el-button
        >
        <el-button type="primary" @click="download()">下载模板</el-button>
        <el-dialog
          title="新建数据库"
          :visible.sync="dialogCreateVisible"
          width="30%"
        >
          <p>请输入数据仓库的名称</p>
          <p>
            <el-input
              v-model="input"
              placeholder="请输入仓库名称"
              style="width: 60%; margin-top: 20px"
            ></el-input>
          </p>
          <span slot="footer" class="dialog-footer">
            <el-button @click="dialogCreateVisible = false">取 消</el-button>
            <el-button type="primary" @click="saveDataStore()">确 定</el-button>
          </span>
        </el-dialog>
        <!--  -->
        <el-dialog title="设置颜色" :visible.sync="colorVisible" width="40%">
          <div style="margin-bottom: 20px">
            <el-select
              v-model="selectDataStore"
              placeholder="请选择数据库"
              :change="changeDataStore()"
            >
              <el-option
                v-for="item in dataStores"
                :key="item.id"
                :label="item.name"
                :value="item.name"
              ></el-option>
            </el-select>
            <el-select
              v-model="selectSce"
              placeholder="请选择场景"
              :change="changeScen()"
            >
              <el-option
                v-for="item in scenariosList"
                :key="item.id"
                :label="item.name"
                :value="item.name"
              ></el-option>
            </el-select>
          </div>
          <p v-for="item in colorList" :key="item.id" class="color-p">
            <span class="color-item1">{{ item.name }}</span>
            <span class="color-item2">
              <input
                v-model="item.color"
                type="text"
                autocomplete="off"
                placeholder="请输入颜色数值"
                class="el-input__inner"
              />
              <span
                class="color-block"
                v-bind:style="{ background: item.color }"
              ></span>
              <span v-if="checkColor(item.color)" class="error-info">
                格式错误!</span
              >
            </span>
          </p>
          <div class="other-color">
            <h5 v-if="otherColorList.length > 0" style="margin-bottom: 10px">
              其他备选颜色
            </h5>
            <span
              v-for="item in otherColorList"
              :key="item.id"
              class="other-color-item"
            >
              <span>{{ item.color }}</span>
              <span
                class="color-block"
                v-bind:style="{ background: item.color }"
              ></span>
            </span>
          </div>
          <span slot="footer" class="dialog-footer">
            <el-button @click="colorVisible = false">取 消</el-button>
            <el-button type="primary" @click="saveColor()">保 存</el-button>
          </span>
        </el-dialog>

        <!--  -->
        <el-dialog
          title="删除数据库"
          :visible.sync="dialogDelVisible"
          width="30%"
        >
          <p>您确实要删除这个数据仓库吗？</p>
          <span slot="footer" class="dialog-footer">
            <el-button @click="dialogDelVisible = false">取 消</el-button>
            <el-button type="primary" @click="del()">确 定</el-button>
          </span>
        </el-dialog>

        <el-dialog
          title="上传数据"
          :visible.sync="dialogUploadVisible"
          width="30%"
        >
          <div
            class="file-item"
            v-for="item of uploadFiles"
            v-bind:key="item.key"
          >
            <el-input
              v-model="item.scenario"
              size="small"
              placeholder="请输入场景名称"
              style="width: 40%; margin-right: 30px"
            ></el-input>
            <input class="file" type="file" @change="getFile($event, item)" />
          </div>
          <div>
            <i
              class="add-btn el-icon-circle-plus-outline"
              @click="addFile()"
            ></i>
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

<style type="text/css" scoped>
@import "../assets/upload.css";
</style>

<script>
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import axios from "axios";
import qs from "qs";

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
      colorVisible: false,
      input: "",
      radio: "",
      uploadFiles: [],
      defaultDataStore: "",
      selectDataStore: "",
      lastDataStore: "",
      scenariosList: [],
      selectSce: "",
      colorList: [],
      otherColorList: [],
      lastScen: "",
      lastDataStore: "",
      selectRadioDataStore: "",
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

    openDel() {
      if (this.radio == "") {
        alert("请选择要删除的数据库");
        return;
      }
      this.dialogDelVisible = true;
    },

    reloadData() {
      if (this.radio == "") {
        alert("请选择要加载的数据库");
        return;
      }
      axios
        .post(
          `${this.baseURL}/api/DataStoreMgmt/preprocess/reload`,
          qs.stringify({ DatastoreName: this.radio })
        )
        .then((res) => {
          console.log(res);
        });
    },

    changeScen() {
      if (this.selectSce == "" || this.selectSce == this.lastScen) {
        return;
      }
      this.lastScen = this.selectSce;
      axios
        .get(
          `${this.baseURL}/api/Config/entitycolor?datastoreName=${encodeURI(
            this.selectDataStore
          )}&scenarioName=${encodeURI(this.selectSce)}`
        )
        .then((response) => {
          this.colorList = [];
          for (let [key, value] of Object.entries(
            response.data.entityColorConfig
          )) {
            this.colorList.push({ name: key, color: value });
          }
          console.log(this.colorList);
          axios.get(`${this.baseURL}/api/Config/colors`).then((res) => {
            this.otherColorList = [];
            for (let [key, value] of Object.entries(res.data.colors)) {
              this.otherColorList.push({ name: key, color: value });
            }
          });
        });
    },

    saveColor() {
      let str = "";
      for (let i = 0; i < this.colorList.length; i++) {
        if (this.checkColor(this.colorList[i].color)) {
          alert("颜色格式错误");
          return false;
        }
        str +=
          "&" +
          encodeURI(this.colorList[i].name) +
          "=" +
          encodeURIComponent(this.colorList[i].color);
      }

      axios
        .post(
          `${
            this.baseURL
          }/api/Config/entitycolor?user=skuser1&datastoreName=${encodeURI(
            this.selectDataStore
          )}&scenarioName=${encodeURI(this.selectSce)}${str}`,
          {}
        )
        .then((res) => {
          alert("颜色设置保存成功");
          this.colorVisible = false;
          this.colorList = [];
          this.selectDataStore = "";
          this.selectSce = "";
          this.otherColorList = [];
          this.lastScen = "";
          this.selectSce = "";
        });
    },

    checkColor(color) {
      return /^#([0-9a-fA-F]{6})$/.test(color) ? false : true;
    },
    //

    getDataStore() {
      // this.radio = this.r
    },
    changeDataStore() {
      if (
        this.selectDataStore == "" ||
        this.selectDataStore == this.lastDataStore
      ) {
        return;
      }
      this.lastDataStore = this.selectDataStore;
      this.getScenarios();
      this.lastScen = "";
      this.selectSce = "";
    },

    getScenarios() {
      this.scenariosList = [];
      axios
        .get(
          `${this.baseURL}/api/Graph/scenarios?datastoreName=${this.selectDataStore}`
        )
        .then((response) => {
          for (let i = 0; i < response.data.scenarioNames.length; i++) {
            this.scenariosList.push({
              id: i,
              name: response.data.scenarioNames[i],
            });
          }
        });
    },
    //

    download() {
      window.location.href = `./SmartKG_KGDesc_Template.xlsx`;
    },

    getList() {
      this.dataStores = [];
      axios.get(`${this.baseURL}/api/DataStoreMgmt`).then((res) => {
        for (let i = 0; i < res.data.datastoreNames.length; i++) {
          this.dataStores.push({
            name: res.data.datastoreNames[i],
          });
        }
      });
    },
    del() {
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
        this.radio = "";
      });
    },

    getFile(event, item) {
      item.file = event.target.files[0];
    },
    openUploadwin(name) {
      if (this.radio == "") {
        alert("请选择要上传的数据库");
        return;
      }
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
      console.log(this.uploadFiles.length);
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
        setTimeout(() => {
          axios
            .post(
              `${this.baseURL}/api/DataStoreMgmt/preprocess/upload`,
              formData,
              config
            )
            .then((res) => {
              if (res.data.success == false) {
                alert(res.data.responseMessage);
                this.dialogUploadVisible = false;
              } else {
                result.push(true);
              }
            });
        }, i * 1000);
      }
      let timer = setInterval(() => {
        if (result.length == this.uploadFiles.length) {
          if (result.every((item) => item == true)) {
            alert("上传成功");
            this.dialogUploadVisible = false;
            this.radio = "";
            let formData = new FormData();
            formData.append("DatastoreName", this.currentDataStore);
            axios
              .post(
                `${this.baseURL}/api/DataStoreMgmt/preprocess/reload`,
                formData
              )
              .then((res) => {});
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


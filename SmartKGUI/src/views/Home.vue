<template>
  <div class="main">
    <div class="chart-page">
      <div id="echart" width="400" height="400"></div>
      <div id="tip">
        <el-popover
          placement="bottom"
          width="400"
          trigger="click"
          style="padding: 0 !important"
        >
          <div class="bot">
            <div class="chat">
              <div class="chat-title">
                <span class="bot-title">SmartKG bot</span>
              </div>
              <div class="history" id="history">
                <div v-for="item of histroyList" v-bind:key="item.key">
                  <div v-if="item.from == 'user'" class="user-info-wrap">
                    <div class="user-info">{{ item.info }}</div>
                    <i class="el-icon-user-solid user-icon"></i>
                  </div>
                  <div v-if="item.from == 'bot'" class="bot-info-wrap">
                    <i class="el-icon-service bot-icon"></i>
                    <div class="bot-info">
                      <pre>{{ item.info }}</pre>
                    </div>
                  </div>
                </div>
              </div>
              <div class="send">
                <div class="chat-input">
                  <input
                    v-model="currentText"
                    type="text"
                    id="curentChat"
                    style="border-radius: 5px"
                    v-on:keyup.enter="send"
                  />
                  <div class="btnsend" @click="send()" title="send">
                    <i class="el-icon-chat-round"></i>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <el-button slot="reference">
            <i class="el-icon-service" style="font-size: 30px"></i>
          </el-button>
        </el-popover>
      </div>
      <div class="chart-wrap">
        <div class="chart-search">
          <div class="chart-search-div">
            <input
              class="chart-search-input"
              type="text"
              placeholder="请输入实体名称"
              v-model="keyWord"
              v-on:input="gotochart"
              v-on:keyup.enter="gotochartEnter"
              ref="search"
            />
            <button
              class="chart-search-btn"
              @click="gotochartSmallBtn()"
            ></button>
          </div>
          <el-select
            v-model="selectDataStore"
            placeholder="请选择数据库"
            style="left: -88px; width: 222px; margin-bottom: 10px"
            :change="changeDataStore()"
          >
            <el-option
              v-for="item in datastoreList"
              :key="item.id"
              :label="item.name"
              :value="item.name"
            ></el-option>
          </el-select>
          <el-select
            v-model="selectSce"
            placeholder="请选择场景"
            style="left: -88px; width: 222px"
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
        <div class="chart-result">
          <p class="title" v-if="isExpand">
            模糊搜索结果信息：共找到
            <span class="result-num">{{ list.length }}</span
            >结果
          </p>
          <ul class="item-list" :class="{ close: !isExpand, expand: isExpand }">
            <li
              class="list-item"
              style="justify-content: left"
              v-for="item of list"
              v-bind:key="item.id"
              :class="{ active: item.id == currentId }"
              @click="getNodes(item.id, item.name)"
              v-html="changeListKey(item.name)"
            ></li>
          </ul>
          <hr v-if="isExpand" />
          <div class="control">
            <span class="control-btn" @click="isExpand = !isExpand">
              全部收起
              <span
                class="arrow"
                :class="{ up: isExpand, down: !isExpand }"
              ></span>
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style type="text/css" scoped>
@import "../assets/home.css";
</style>

<script>
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
import axios from "axios";
import echarts from "echarts";

export default {
  name: "Home",
  data() {
    return {
      keyWord: "",
      hotkeyList: [],
      isExpand: true,
      list: [],
      nodes: [],
      edges: [],
      sendTime: new Date().getTime(),
      timer: null,
      charts: null,
      currentId: "",
      baseURL: window.urlapi,
      scenariosList: [],
      datastoreList: [],
      colorList: [],
      selectSce: "",
      lastScen: "",
      selectDataStore: "",
      lastDataStore: "",
      histroyList: [],
      currentText: "",
      sessionId: "",
    };
  },
  components: {},
  methods: {
    send() {
      if (this.currentText.trim() == "") {
        return;
      }
      axios
        .post(`${this.baseURL}/api/bot`, {
          userId: "",
          sessionId: this.sessionId,
          datastoreName: this.selectDataStore,
          query: this.currentText,
        })
        .then((res) => {
          this.sessionId = res.data.sessionId;
          this.histroyList.push({
            from: "bot",
            info: res.data.result.responseMessage,
            key: new Date().valueOf(),
          });
          setTimeout(() => {
            document.querySelector("#history").scrollTop = 99999;
          }, 150);
        });
      this.histroyList.push({
        from: "user",
        info: this.currentText,
        key: new Date().valueOf(),
      });
      this.currentText = "";
      setTimeout(() => {
        document.querySelector("#history").scrollTop = 99999;
      }, 150);
    },
    changeScen() {
      
      

      if (this.selectSce == "" || this.selectSce == this.lastScen) {
        return;
      }
      this.lastScen = this.selectSce;
      // axios
      //   .get(
      //     `${this.baseURL}/api/Graph/visulize?datastoreName=${encodeURI(
      //       this.selectDataStore
      //     )}&scenarioName=${encodeURI(this.selectSce)}`
      //   )
      //   .then((res) => {
      //     this.nodes = res.data.nodes;
      //     this.edges = res.data.relations;
      //     this.process();
      //     this.generate();
      //   });
      axios
        .get(
          `${this.baseURL}/api/Config/entitycolor?datastoreName=${encodeURI(
            this.selectDataStore
          )}&scenarioName=${encodeURI(this.selectSce)}`
        )
        .then((response) => {
          for (let [key, value] of Object.entries(
            response.data.entityColorConfig
          )) {
            this.colorList.push({ name: key, color: value });
          }
          axios
            .get(
              `${this.baseURL}/api/Graph/visulize?datastoreName=${encodeURI(
                this.selectDataStore
              )}&scenarioName=${encodeURI(this.selectSce)}`
            )
            .then((res) => {
              this.nodes = res.data.nodes;
              this.edges = res.data.relations;
              this.process();
              this.generate();
            });
        });
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
    gotochartEnter() {
      axios
        .get(`${this.baseURL}/api/Search?keyword=${encodeURI(this.keyWord)}`)
        .then((response) => {
          this.currentId = response.data.nodes[0].id;
          axios
            .get(`${this.baseURL}/api/Search/${response.data.nodes[0].id}`)
            .then((res) => {
              this.nodes = res.data.nodes;
              this.edges = res.data.relations;
              this.process();
              this.generate();
            });
        });
    },
    changeListKey(item) {
      return item.replace(
        new RegExp(this.keyWord, "g"),
        `<span style='color:red'>${this.keyWord}</span>`
      );
    },
    gotochartSmallBtn() {
      this.gotochartEnter();
      this.gotochart();
    },
    gotochart() {
      if (this.keyWord != "") {
        setTimeout(() => {
          this.$refs.search.focus();
          this.$refs.search.selectionStart = 100;
          this.$refs.search.selectionEnd = 100;
        }, 10);
      }
      this.timer = setTimeout(() => {
        let now = new Date().getTime();
        if (now - this.sendTime < 600 || this.keyWord == "") {
          clearTimeout(this.timer);
          return;
        }
        this.sendTime = new Date().getTime();
        if (this.keyWord == "") {
          return;
        }
        axios
          .get(
            `${this.baseURL}/api/Graph/search?datastoreName=${this.selectDataStore}&keyword=${this.keyWord}`
          )
          .then((response) => {
            if (response.data.nodes === null) {
              this.list = [];
              this.charts.dispose();
              this.charts.hideLoading();
            } else {
              this.list = response.data.nodes;
            }
          });
      }, 2000);
    },
    getNodes(id, name) {
      // this.lastScen = "";
      // this.selectSce = "";
      this.currentId = id;
      axios
        .get(
          `${this.baseURL}/api/Graph/relations/${id}?datastoreName=${this.selectDataStore}`
        )
        .then((res) => {
          this.nodes = res.data.nodes;
          this.edges = res.data.relations;
          this.process();
          this.generate();
          this.charts.hideLoading();
        });
    },
    generate() {
      this.charts = echarts.init(document.getElementById("echart"));
      var option = {
        title: {
          top: "bottom",
          left: "right",
        },
        tooltip: {
          trigger: "item",
          formatter: (params, ticket) => {
            if (params.data.fullname == undefined) {
              return;
            }
            let str = "";
            for (let count = 0; count < params.data.fullname.length; count++) {
              str += params.data.fullname[count];
              if (count % 20 == 0 && count != 0) {
                str += "<br/>";
              }
            }
            return str;
          },
          textStyle: {
            width: "100px",
          },
          extraCssText: "text-align:left;",
        },
        draggable: true,
        series: [
          {
            nodeScaleRatio: 0,
            zoom: 1,
            animation: false,
            name: "Les Miserables",
            type: "graph",
            edgeSymbol: ["", "arrow"],
            focusNodeAdjacency: true,
            layout: "force",
            force: {
              initLayout: false,
              layoutAnimation: false,
              repulsion: 300,
              edgeLength: 140,
              gravity: 0.1,
            },
            edgeLabel: {
              show: true,
            },
            data: this.nodes,
            links: this.edges,
            roam: true,
            label: {
              color: "#000",
              normal: {
                position: "right",
              },
            },
            lineStyle: {
              normal: {
                curveness: 0.2,
              },
            },
          },
        ],
      };
      this.charts.setOption(option);
      if(this.charts){
        this.charts.off('click');
      }
      this.charts.on("click", (e) => {
        this.charts = echarts.init(document.getElementById("echart"));
        this.charts.showLoading({
          text: "正在加载数据",
          color: "none",
        });
        this.getChildNode(e.data).then(() => {
          this.process();
          option.series[0].data = this.nodes;
          option.series[0].links = this.edges;
          this.charts.setOption(option);
          this.charts.resize({
            width: "auto",
          });
          this.charts.hideLoading();
        });
      });
    },
    getChildNode(node) {
      console.log(3333);
      let url = "";
      if (/属性/.test(node.info)) {
        url = `${this.baseURL}/api/Search/property?name=${encodeURI(
          node.displayName
        )}&value=${encodeURI(node.name)}`;
      } else {
        url = `${this.baseURL}/api/Graph/relations/${node.id}?datastoreName=${this.selectDataStore}`;
      }
      console.log(url, node);
      let promise = new Promise((resolve, reject) => {
        axios({
          method: "get",
          url,
        }).then((res) => {
            console.log(res, 62);
            this.nodes = res.data.nodes;
            this.edges = res.data.relations;
            this.process();
            resolve();
        });
      });
      return promise;
    },
    process() {
      for (let i = 0; i < this.nodes.length; i++) {
        this.nodes[i].draggable = false;
        if (this.nodes[i].name.length > 7) {
          if (!this.nodes[i].fullname) {
            this.nodes[i].fullname = this.nodes[i].name;
            this.nodes[i].name = this.nodes[i].name.substring(0, 6) + "...";
          }
        }
        if (this.nodes[i].info == undefined) {
          this.nodes[i].info = this.nodes[i].label;
        }
        this.nodes[i].label = {
          show: true,
          position: "bottom",
          width: "30",
          color: "#000",
        };
        this.nodes[i].symbolSize = 30;
        if (this.nodes[i].name == "true") {
          this.nodes[i].symbolSize = 10;
          this.nodes[i].label = { show: false };
        }
        this.nodes[i].itemStyle = {
          color: "#cccccc",
          borderColor: "#cccccc",
          shadowColor: "rgba(0, 0, 0, 0.5)",
          shadowBlur: 3,
        };
        for (let k = 0; k < this.colorList.length; k++) {
          if (this.nodes[i].info == this.colorList[k].name) {
            this.nodes[i].itemStyle = {
              color: this.colorList[k].color,
              borderColor: "#ffffff",
              shadowColor: "rgba(0, 0, 0, 0.5)",
              shadowBlur: 3,
            };
            this.nodes[i].label.color = this.colorList[k].color;
          }
        }
      }
      for (let i = 0; i < this.edges.length; i++) {
        this.edges[i].source = this.edges[i].sourceId;
        this.edges[i].target = this.edges[i].targetId;
        for (let j = 0; j < this.nodes.length; j++) {
          if (this.edges[i].target == this.nodes[j].id)
            this.edges[i].lineStyle = {
              width: 2,
              color: "#000000",
            };
        }
        this.edges[i].label = {
          formatter: this.edges[i].value,
        };
      }
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
    getDataStore() {
      this.datastoreList = [];
      axios.get(`${this.baseURL}/api/DataStoreMgmt`).then((response) => {
        console.log(response);
        for (let i = 0; i < response.data.datastoreNames.length; i++) {
          this.datastoreList.push({
            id: i,
            name: response.data.datastoreNames[i],
          });
        }
      });
    },
  },
  mounted() {
    this.getDataStore();
  },
};
</script>


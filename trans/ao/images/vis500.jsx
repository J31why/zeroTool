#target photoshop


var Texts = 
[
    "Ｄ∴Ｇ教团 阿勒泰尔据点旧址","",
    "克洛斯贝尔自治州东方","唐古拉姆丘陵——",
    "利贝尔王国上空——","高速巡洋舰《埃尔赛尤》",
    "埃雷波尼亚帝国  帝国某处 ——","",
    "３４Ｆ 会议相关者楼层","",
    "３５Ｆ 国际会议楼层","",
    "３６Ｆ ＶＩＰ楼层","",
    "红色星座所属 强袭登陆舰","《贝奥武夫号》——",
    "七耀教会 星杯骑士团所属","特殊作战艇《梅尔卡巴》——",
    "梅尔卡巴伍号机","",
    "梅尔卡巴玖号机","",
    "战略巨神兵级 战略傀儡最终型","《神机永世》TYPE-β",
    "贝尔加德门 货物月台","",
    "唐古拉姆丘陵","无线信号放大器地点上空——",
    "埃雷波尼亚帝国 政府代表","《铁血宰相》吉利亚斯・奥斯本",
    "埃雷波尼亚帝国 皇帝代理","奥利巴特・莱泽・亚诺尔皇子",
    "卡尔瓦德共和国 政府代表","萨谬尔・洛克史密斯总统",
    "利贝尔王国 王储","克萝蒂雅・冯・奥赛雷丝",
    "雷米菲利亚公国 国家元首", "阿尔伯特・冯・巴托罗谬大公",
    "３４Ｆ 共同会议室","",
    "克洛斯贝尔自治州 共同代表","亨利・麦克道尔议长",
    "克洛斯贝尔自治州 共同代表","迪塔・库罗伊斯市长",
];

var baseName ="c_vis5";
var outputFolder = "C:\\Users\\Jelly\\Desktop\\out";
var doc = app.activeDocument;

var line1TextLayer = null;
var line2TextLayer = null;
for (var i = 0; i < doc.layers.length; i++) {

    if (doc.layers[i].name == "line1" && doc.layers[i].kind == LayerKind.TEXT) {
        line1TextLayer = doc.layers[i];
    }
    else if (doc.layers[i].name == "line2" && doc.layers[i].kind == LayerKind.TEXT){
        line2TextLayer = doc.layers[i];
    }
}

for (var i = 0; i < Texts.length; i+=2) {

    line1TextLayer.textItem.contents = Texts[i];
    line2TextLayer.textItem.contents = Texts[i+1];
    var index= i/2;
    var number = (index < 10 ? "0" : "") + index;
    var fileName = baseName + number;
    var saveFile = new File(outputFolder + "\\"+fileName+".png");
    var exportOptions = new ExportOptionsSaveForWeb();
    exportOptions.format = SaveDocumentType.PNG;
    exportOptions.PNG8 = false;
    exportOptions.quality = 100;
    exportOptions.transparency = true;
    exportOptions.includeProfile = false;
    exportOptions.optimized = true;
    
    doc.exportDocument(saveFile, ExportType.SAVEFORWEB, exportOptions);
    //alert("已输出 "+cnTexts[i]);
}
line1TextLayer.textItem.contents = "line1";
line2TextLayer.textItem.contents = "line2";


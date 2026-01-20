#target photoshop


var cnTexts = 
[
    "克洛斯贝尔车站",       "站前大道",         "中央广场",          "西侧大道",            "东侧大道",
    "行政区",              "ＩＢＣ",           "欢乐街",            "小巷",                "住宅区",
    "旧市街",              "剧团《彩虹》",      "鲁巴彻商会",        "黑月贸易公司",        "阿尔摩利卡村",
    "圣乌尔斯拉医科大学",   "罗赞贝尔克工房",    "矿山镇玛因兹",       "贝尔加德门",          "唐古拉姆门",
    "游憩区米修拉姆",       "哈尔特曼议长宅邸",  "东克洛斯贝尔街道",   "西克洛斯贝尔街道",     "乌尔斯拉小径",
    "阿尔摩利卡古道",       "玛因兹山道",       "古战场",            "星见之塔",            "月之僧院",
    "太阳堡垒",             "港湾区",           "克洛斯贝尔机场",    "克洛斯贝尔大教堂",    "特务支援课",
];
var enTexts = 
[
    "Crossbell Station",            "Station Street",               "Central Square",           "West Street",              "East Street",
    "Administrative District",      "IBC",                          "Entertainment District",   "Back Alley",               "Residential District",
    "Downtown District",            "Arc en Ciel",                  "Revache & Co.",            "Heiyue Trading, Ltd.",     "Armorica Village",
    "St. Ursula Medical College",   "Rosenberg Studio",             "Mainz Mining Village",     "Bellguard Gate",           "Tangram Gate",
    "Mishelam Resort",              "Speaker Hartmann's Mansion",   "East Crossbell Highway",   "West Crossbell Highway",   "Ursula Road",
    "Old Armorica Road",            "Mainz Mountain Path",          "Ancient Battlefield",      "Stargazer's Tower",        "Moon Temple",
    "Sun Fort",                     "Harbor District",              "Crossbell Airport",        "Crossbell Cathedral",      "Special Support Section",
];
var baseName ="c_plac";
var outputFolder = "C:\\Users\\Jelly\\Desktop\\out";
var doc = app.activeDocument;

var cnTextLayer = null;
var enTextLayer = null;
var bg1 = null;
var bg2 = null;
for (var i = 0; i < doc.layers.length; i++) {
    if (doc.layers[i].name == "cn" && doc.layers[i].kind == LayerKind.TEXT) {
        cnTextLayer = doc.layers[i];
    }
    else if (doc.layers[i].name == "en" && doc.layers[i].kind == LayerKind.TEXT){
        enTextLayer = doc.layers[i];
    }
    else if (doc.layers[i].name == "bg1"){
        bg1 = doc.layers[i];
    }
    else if (doc.layers[i].name == "bg2"){
        bg2 = doc.layers[i];
    }
}

for (var i = 20; i < 21; i++) {
    cnTextLayer.textItem.font = enTextLayer.textItem.font;
    cnTextLayer.textItem.contents = cnTexts[i];
    enTextLayer.textItem.contents = enTexts[i];

    if(i == 20)
    {
        bg1.visible = false;
        bg2.visible = true;
    }
    else{
        bg1.visible = true;
        bg2.visible = false;
    }
    var number = (i < 10 ? "0" : "") + i;
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
cnTextLayer.textItem.contents = "中文文本";
enTextLayer.textItem.contents = "English Text";


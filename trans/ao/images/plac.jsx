#target photoshop


var cnTexts = 
[
    "克洛斯贝尔车站",       "站前大道",         "中央广场",          "西侧大道",                "东侧大道",
    "行政区",              "ＩＢＣ",           "欢乐街",            "小巷",                    "住宅区",
    "旧市街",              "剧团《彩虹》",      "鲁巴彻商会",        "黑月贸易公司",            "阿尔摩利卡村",
    "圣乌尔斯拉医科大学",   "罗赞贝尔克工房",    "矿山镇玛因兹",       "贝尔加德门",             "唐古拉姆门",
    "游憩区米修拉姆",       "哈尔特曼议长宅邸",  "东克洛斯贝尔街道",   "西克洛斯贝尔街道",        "乌尔斯拉小径",
    "阿尔摩利卡古道",       "玛因兹山道",       "古战场",            "星见之塔",                "月之僧院",
    "太阳堡垒",             "港湾区",           "克洛斯贝尔机场",    "克洛斯贝尔大教堂",        "特务支援课",
    "阿勒泰尔市",           "诺克斯森林道",     "警察学校",          "兰花塔",                  "克洛斯贝尔车站内",
    "34F 会议相关者楼层",   "34F 共同会议室",   "35F 国际会议楼层",   "36F VIP楼层",            "货物月台",
    "湖水浴场",             "米修拉姆奇幻乐园", "镜之城",            "大摩天轮",                "恐怖云霄飞车",
    "阿勒泰尔旧址",         "湿地",             "旧矿山",           "地下区域Ｄ区块",           "地下区域Ｃ区块",
    "诺克斯树海",           "中枢区块",         "碧之大树－神域－",  "象之领域",                "色之领域",
    "业之领域",             "戒之领域",         "通往尽头的道路",     "零的世界",               "星辰之间",
    "唐古拉姆丘陵",         "加雷利亚要塞",     "新布朗",             "碧之大树－尽头－",        "无的世界",
    "诺克斯拘留所",         "叉路交流道",       "旧矿山・废坑入口",    "米修拉姆迎宾馆"
];
var enTexts = 
[
    "Crossbell Station",            "Station Street",               "Central Square",                   "West Street",                  "East Street",
    "Administrative District",      "IBC",                          "Entertainment District",           "Back Alley",                   "Residential District",
    "Downtown District",            "Arc en Ciel",                  "Revache & Co.",                    "Heiyue Trading, Ltd.",         "Armorica Village",
    "St. Ursula Medical College",   "Rosenberg Studio",             "Mainz Mining Village",             "Bellguard Gate",               "Tangram Gate",
    "Mishelam Resort",              "Speaker Hartmann's Mansion",   "East Crossbell Highway",           "West Crossbell Highway",       "Ursula Road",
    "Old Armorica Road",            "Mainz Mountain Path",          "Ancient Battlefield",              "Stargazer's Tower",            "Moon Temple",
    "Sun Fort",                     "Harbor District",              "Crossbell Airport",                "Crossbell Cathedral",          "Special Support Section",
    "Altair",                       "Knox Forest Road",             "Crossbell Police Academy",         "Orchis Tower",                 "",
    "",                             "",                             "",                                 "",                             "",
    "Lakeside Beach",               "Mishelam Wonderland",          "Castle of Mirrors",                "Grand Wheel",                  "Horror Coaster",
    "Former Altair Lodge",          "Wetlands",                     "Old Mine",                         "Geofront - D Sector",          "Geofront - C Sector",
    "Knox Forest",                  "Mystic Core",                  "Azure Tree - Holy Precincts",      "Domain of Vanity",             "Domain of Passion",
    "Domain of Fate",               "Domain of Penance",            "Road to the Farthest End",         "World of Zero",                "Celestial Globe",
    "Tangram Hill",                 "Garrelia Fortress",            "Neue Blanc",                       "Azure Tree - The Farthest",    "World of Ain",
    "Knox Prison",                  "Junction Area",                "Enterance of Abandoned Mineroad",  "Mishelam Guest House"
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

for (var i = 0; i < cnTexts.length; i++) {
    if(i==21) continue;
    cnTextLayer.textItem.font = enTextLayer.textItem.font;
    cnTextLayer.textItem.contents = cnTexts[i];
    enTextLayer.textItem.contents = enTexts[i];

    if(i == 20|| i>=45 && i <=49 )
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


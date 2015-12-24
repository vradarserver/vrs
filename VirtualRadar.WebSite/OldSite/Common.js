var __IsIpad;
function isIpad()
{
    if(__IsIpad === undefined) __IsIpad = navigator.userAgent.indexOf('iPad') !== -1;
    return __IsIpad;
};

var __IsIphone;
function isIphone()
{
    if(__IsIphone === undefined) __IsIphone = navigator.userAgent.indexOf('iPhone') !== -1;
    return __IsIphone;
}

var __IsAndroid;
function isAndroid()
{
    if(__IsAndroid === undefined) __IsAndroid = navigator.userAgent.indexOf('Android') !== -1;
    return __IsAndroid;
}

function includeJavascript(url)
{
    document.write('<script type="text/javascript" src="'+ url + '"></script>'); 
}

function swapArrayElements(arr, idx1, idx2)
{
    if(arr !== null && arr !== undefined && idx1 !== idx2 && idx1 >= 0 && idx2 >= 0 && idx1 < arr.length && idx2 < arr.length) {
        var value1 = arr[idx1];
        arr[idx1] = arr[idx2];
        arr[idx2] = value1;
    }
}

function replaceDateConstructors(json)
{
    return json.replace(/\"\\\/Date\(([\d\+\-]+)\)\\\/\"/g, 'new Date($1)');
}

function target(name)
{
    var targetOverride = getPageParameterValue(undefined, 'forceFrame');
    return ' target="' + (targetOverride ? targetOverride : name) + '"';
}

function setClass(element, classText)
{
    element.setAttribute('class', classText);
    element.setAttribute('className', classText);
}

function createElement(elementType, className, parentElement, id)
{
    var result = document.createElement(elementType);
    if(className !== null && className !== undefined) setClass(result, className);
    if(parentElement !== null && parentElement !== undefined) parentElement.appendChild(result);
    if(id !== undefined && id !== null) result.setAttribute('id', id);

    return result;
}

function createInput(inputType, name, className, parentElement)
{
    var result = null;
    try {
        result = createElement('input', className, parentElement);
        result.setAttribute('type', inputType);
        result.setAttribute('name', name);
    } catch(e) {
        result = createElement('<input type="' + inputType + '" name="' + name + '">', className, parentElement);
    }

    return result;
}

function createLink(href, className, parentElement, id, content)
{
    var result = createElement('a', className, parentElement, id);
    result.setAttribute('href', href);
    if(content !== undefined && content !== null) result.innerHTML = content;

    return result;
}

function createImage(source, width, height, tooltip)
{
    var result = new Image();
    result.src = source;
    if(width !== undefined) result.width = width;
    if(height !== undefined) result.height = height;
    if(tooltip !== undefined) result.alt = tooltip;

    return result;
}

function appendClass(element, classText)
{
    if(classText !== null && classText !== undefined && classText.length > 0) {
        var currentClass = element.getAttribute("class");
        if(currentClass === undefined || currentClass === null) currentClass = "";

        if(currentClass.length > 0) currentClass += ' ';
        currentClass += classText;

        setClass(element, currentClass);
    }
}

function intToString(intValue, width)
{
    if(width === undefined) width = 0;
    var result = "";
    var intString = intValue.toString();
    for(var zerosNeeded = width - intString.length;zerosNeeded > 0;--zerosNeeded) {
        result += "0";
    }
    result += intString;

    return result;
}

function trim(text)
{
	return text.replace(/^\s+|\s+$/g, "");
}

function capitaliseSentence(text)
{
    if(text && text.length > 0) {
        if(text.length === 1) text = text.toUpperCase();
        else text = text.charAt(0).toUpperCase() + text.substr(1);
    }

    return text;
}

function getTimeSpan(start, end)
{
    var span = end.getTime() - start.getTime();
    var hours = Math.floor(span / 3600000);
    span -= hours * 3600000;
    var minutes = Math.floor(span / 60000);
    span -= minutes * 60000;
    var seconds = Math.floor(span / 1000);
    span -= seconds * 1000;

    return { hours: hours, minutes: minutes, seconds: seconds, milliseconds: span };
}

function dayOfWeek(value, longForm)
{
    var result = null;
    switch(value) {
        case 0: result = !longForm ? "Sun" : "Sunday"; break;
        case 1: result = !longForm ? "Mon" : "Monday"; break;
        case 2: result = !longForm ? "Tue" : "Tuesday"; break;
        case 3: result = !longForm ? "Wed" : "Wednesday"; break;
        case 4: result = !longForm ? "Thu" : "Thursday"; break;
        case 5: result = !longForm ? "Fri" : "Friday"; break;
        case 6: result = !longForm ? "Sat" : "Saturday"; break;
    }

    return result;
}

function monthOfYear(value, longForm)
{
    var result = null;
    switch(value) {
        case 0: result = !longForm ? "Jan" : "January"; break;
        case 1: result = !longForm ? "Feb" : "February"; break;
        case 2: result = !longForm ? "Mar" : "March"; break;
        case 3: result = !longForm ? "Apr" : "April"; break;
        case 4: result = !longForm ? "May" : "May"; break;
        case 5: result = !longForm ? "Jun" : "June"; break;
        case 6: result = !longForm ? "Jul" : "July"; break;
        case 7: result = !longForm ? "Aug" : "August"; break;
        case 8: result = !longForm ? "Sep" : "September"; break;
        case 9: result = !longForm ? "Oct" : "October"; break;
        case 10: result = !longForm ? "Nov" : "November"; break;
        case 11: result = !longForm ? "Dec" : "December"; break;
    }

    return result;
}

function findStylesheet(selectorText)
{
    var result;

    var length = document.styleSheets.length;
    for(var i = 0;i < length;++i) {
        if(getCSSRule(i, selectorText)) {
            result = i;
            break;
        }
    }

    return result;
}

function getCSSRule(styleSheetIndex, selectorText)
{
    var result = null;
    var rules = getCssRules(styleSheetIndex);
    if(rules !== null && rules !== undefined) {
        selectorText = selectorText.toUpperCase();
        for(var i = 0;i < rules.length;++i) {
            var rule = rules[i];
            if(rule.selectorText && rule.selectorText.toUpperCase() === selectorText) {
                result = rule;
                break;
            }
        }
    }

    return result;
};

function getCssRules(styleSheetIndex)
{
    try {
        var result = document.styleSheets[styleSheetIndex].cssRules;
        if(!result) result = document.styleSheets[styleSheetIndex].rules;
    } catch(ex) {
        var result = null;
    }
        
    return result;
};


function getArticle(word)
{
    var result = 'a';
    if(word !== null && word !== undefined && word.length > 0) {
        switch(word.toUpperCase()[0]) {
            case 'A':
            case 'E':
            case 'I':
            case 'O':
            case 'U':
                result = 'an';
                break;
        }
    }

    return result;
}

function getTrackDescription(track) {
    var result = "";
    if(track !== null) {
        if(track >= 337.5 || track < 22.5) result = "north";
        else if(track >= 22.5 && track < 67.5) result = "northeast";
        else if(track >= 67.5 && track < 112.5) result = "east";
        else if(track >= 112.5 && track < 157.5) result = "southeast";
        else if(track >= 157.5 && track < 202.5) result = "south";
        else if(track >= 202.5 && track < 247.5) result = "southwest";
        else if(track >= 247.5 && track < 292.5) result = "west";
        else if(track >= 292.5 && track < 337.5) result = "northwest";
    }

    return result;
}

function formatAirframesOrgRegistration(registration)
{
    return registration ? registration.replace('-', '') : null;
}

function formatCountry(country, isMilitary, ignoreNone)
{
    var result = '';

    if(country) result = country;
    if(isMilitary) {
        if(result.length === 0) result = 'Unknown';
        result += ' (Military)';
    }

    if(!ignoreNone && result.length == 0) result = 'Unknown';

    return result;
}

function formatEngines(engines, engineType)
{
    var result = '';

    if(engines) {
        switch(engines) {
            case 'C':   result = 'coupled'; break;
            case '1':   result = 'single'; break;
            case '2':   result = 'twin'; break;
            default:    result = engines; break;
        }

        if(engineType) result += (result == '' ? '' : ' ') + formatEngineType(engineType, true);
    }

    return result;
};

function formatEngineType(engineType, ignoreNone)
{
    var result = '';
    if(engineType) {
        switch(engineType) {
            case 0: if(!ignoreNone) result = 'none'; break;
            case 1: result = 'piston'; break;
            case 2: result = 'turbo'; break;
            case 3: result = 'jet'; break;
            case 4: result = 'electric'; break;
        }
    }

    return result;
};

function formatIcao8643Details(engines, engineType, species, wtc)
{
    var result = '';

    if(engines || engineType) result = formatEngines(engines, engineType);
    if(result !== '') result += ' engined';
    if(wtc) result += (result == '' ? '' : ' ') + formatWakeTurbulenceCategory(wtc, true, false);
    if(species) result += (result == '' ? '' : ' ') + formatSpecies(species, true);

    return result;
};

function formatSpecies(species, ignoreNone)
{
    var result = '';
    if(species) {
        switch(species) {
            case 0: if(!ignoreNone) result = 'none'; break;
            case 1: result = 'landplane'; break;
            case 2: result = 'seaplane'; break;
            case 3: result = 'amphibian'; break;
            case 4: result = 'helicopter'; break;
            case 5: result = 'gyrocopter'; break;
            case 6: result = 'tiltwing'; break;
        }
    }

    return result;
};

function formatWakeTurbulenceCategory(wtc, ignoreNone, expandedDescription)
{
    var result = '';
    if(wtc) {
        switch(wtc) {
            case 0: if(!ignoreNone) result = 'none'; break;
            case 1: result = 'light'; break;
            case 2: result = 'medium'; break;
            case 3: result = 'heavy'; break;
        }

        if(expandedDescription && result.length > 0) {
            switch(wtc) {
                case 0: break;
                case 1: result += ' (up to 7 tons)'; break;
                case 2: result += ' (up to 135 tons)'; break;
                case 3: result += ' (over 135 tons)'; break;
            }
        }
    }

    return result;
};

function getRouteAirport(route)
{
    var result = route;
    if(result && result.length) {
        var separator = result.indexOf(' ');
        if(separator !== -1) result = result.substr(0, separator);
    }

    return result;
};

function getPageParameters()
{
    var result = [];
    if(location.search !== null) {
        var nameValues = location.search.split(/[\?&]/);
        var length = nameValues.length;
        for(var i = 0;i < length;++i) {
            var nameValue = nameValues[i];
            if(nameValue.length > 0) {
                var splitPosn = nameValue.indexOf('=');
                var name = splitPosn == -1 ? nameValue : nameValue.slice(0, splitPosn);
                var value = splitPosn == -1 || splitPosn + 1 >= nameValue.length ? null : nameValue.slice(splitPosn + 1);
                if(name) name = unescape(name);
                if(value) value = unescape(value);
                result.push({ name: name, value: value});
            }
        }
    }

    return result;
}

function getPageParameterValue(parameters, name)
{
    name = name.toUpperCase();

    if(!parameters) parameters = getPageParameters();

    var result = null;
    var length = parameters.length;
    for(var i = 0;i < length;++i) {
        var param = parameters[i];
        if(param.name.toUpperCase() === name) {
            result = param.value;
            break;
        }
    }

    return result;
}

function writeCookie(name, value, days)
{
    if(days === undefined) days = 365;

    var date = new Date();
    date.setTime(date.getTime() + (days*24*60*60*1000));
    var expires = '; expires=' + date.toGMTString();

    document.cookie = name + '=' + value + expires + '; path=/';
}

function eraseCookie(name)
{
    writeCookie(name, "", -1);
}

function readCookieValues()
{
    var result = [];

    var values = document.cookie.split(";");
    var length = values.length;
    for(var i = 0;i < length;++i) {
        var value = values[i];
        while(value.charAt(0) === ' ') value = value.substring(1, value.length);
        var pair = value.split("=");
        if(pair.length === 1) pair.push(null);
        if(pair[1] === "null") pair[1] = null;
        result.push({name: pair[0], value: pair[1]});
    }

    return result;
}

function extractCookieValue(valuesList, name)
{
    var result = null;
    var length = valuesList.length;
    for(var i = 0;i < length;++i) {
        var value = valuesList[i];
        if(value.name === name) {
            result = value.value;
            break;
        }
    }

    return result;
}

function writeStringArrayToString(arr)
{
    var result = '';
    var length = arr.length;
    for(var i = 0;i < length;++i) {
        if(result.length > 0) result += ' ';
        result += '"' + encodeURI(arr[i]) + '"';
    }
    return result;
}

function writeNumberArrayToString(arr)
{
    var result = '';
    var length = arr.length;
    for(var i = 0;i < length;++i) {
        if(result.length > 0) result += ' ';
        result += arr[i].toString();
    }

    return result;
}

function readStringArrayFromString(str)
{
    var result = [];
    if(str !== null && str !== undefined) {
        var chunks = str.split(' ');
        var length = chunks.length;
        for(var i = 0;i < length;++i) {
            var chunk = chunks[i];
            if(chunk.length >= 2) result.push(chunk.substr(1, chunk.length - 2));
        }
    }
    return result;
}

function readNumberArrayFromString(str)
{
    var result = [];
    if(str !== null && str !== undefined) {
        var chunks = str.split(' ');
        var length = chunks.length;
        for(var i = 0;i < length;++i) {
            result.push(Number(chunks[i]));
        }
    }

    return result;
}

function nameValueCollection()
{
    var that = this;
    var mNames = [];
    var mValues = [];

    this.getLength = function() { return mNames.length; };

    function findIndex(name)
    {
        var result = -1;
        var length = mNames.length;
        for(var i = 0;i < length;++i) {
            if(mNames[i] == name) {
                result = i;
                break;
            }
        }

        return result;
    };

    this.pushValue = function(name, value)
    {
        mNames.push(name);
        mValues.push(value);
    }

    this.setValue = function(name, value)
    {
        var idx = findIndex(name);
        if(idx !== -1) mValues[idx] = value;
        else that.pushValue(name, value);
    };

    this.getValue = function(name)
    {
        var idx = findIndex(name);
        return idx === -1 ? null : mValues[idx];
    };

    this.getValueAsString = function(name, defaultValue)
    {
        var text = that.getValue(name);
        return text === null ? defaultValue : text;
    };

    this.getValueAsNumber = function(name, defaultValue)
    {
        var text = that.getValue(name);
        return text === null ? defaultValue : Number(text);
    };

    this.getValueAsBool = function(name, defaultValue)
    {
        var text = that.getValue(name);
        return text === null ? defaultValue : text !== 'false';
    };

    this.getNameAt = function(idx) { return mNames[idx]; };
    this.getValueAt = function(idx) { return mValues[idx]; };
    this.getValueAsNumberAt = function(idx) { return Number(that.getValueAt(idx)); }
    this.getValueAsBoolAt = function(idx) { return that.getValueAt(idx) !== 'false'; }

    this.toString = function()
    {
        var result = '';
        var length = mNames.length;
        for(var i = 0;i < length;++i) {
            var name = mNames[i];
            var value = mValues[i];
            if(name !== null && name !== undefined && name.length > 0 && value !== null && value !== undefined) {
                if(result.length > 0) result += ' ';
                name = encodeURI(name);
                value = '"' + encodeURI(value) + '"'
                result += name + ' ' + value;
            }
        }

        return result;
    };

    this.fromString = function(text)
    {
        mNames = [];
        mValues = [];
        if(text !== null && text !== undefined && text.length > 0) {
            var chunks = text.split(' ');
            var length = chunks.length;
            if(length % 2 === 0) {
                for(var c = 0;c < length;c++) {
                    var name = chunks[c];
                    var value = chunks[++c];
                    if(value.length >= 2) {
                        name = decodeURI(name);
                        value = value.substr(1, value.length - 2);
                        value = decodeURI(value);
                        mNames.push(name);
                        mValues.push(value);
                    }
                }
            }
        }
    };
};

function setRadioValue(radioNodeList, valueToCheck)
{
    var length = radioNodeList.length;
    for(var i = 0;i < length;++i) {
        var radio = radioNodeList[i];
        radio.checked = radio.value === valueToCheck;
    }
}

function getRadioValue(radioNodeList)
{
    var result = "";

    var length = radioNodeList.length;
    for(var i = 0;i < length;++i) {
        var radio = radioNodeList[i];
        if(radio.checked) {
            result = radio.value;
            break;
        }
    }

    return result;
}

function createOptionElement(selectElement, value, text)
{
    var optionElement = document.createElement("option");
    optionElement.setAttribute("value", value);
    optionElement.innerHTML = text;
    selectElement.appendChild(optionElement);
}

function getFormInputElementByName(form, name)
{
    var result = null;

    for(var c = 0;c < form.length;c++) {
        var element = form[c];
        if(element.name === name) {
            result = element;
            break;
        }
    }

    return result;
};

function normaliseHtml(text)
{
    var result = text;
    if(result && result.length) {
        var length = result.length;
        for(var i = 0;i < length;++i) {
            switch(result[i]) {
                case '&':   result = replaceCharacter(result, i, '&amp;'); i += 4; break;
                case '<':   result = replaceCharacter(result, i, '&lt;');  i += 3; break;
                case '>':   result = replaceCharacter(result, i, '&gt;');  i += 3; break;
            }
        }
    }
    
    return result;
};

function replaceCharacter(text, index, replacement)
{
    var result = text;
    if(result && replacement && index < result.length) {
        result = text.slice(0, index) + replacement;
        if(index + 1 < result.length) result += text.slice(index + 1);
    }
    
    return result;
};
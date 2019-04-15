function GenerateId()
{
	var array = new Uint32Array(1);
	var id = window.crypto.getRandomValues(array);
	return array.join();
}

/*
	定义失效类
*/
function FunctionFailure(name)
{
	this.id = GenerateId();
	this.name = name;
	this.description = "";

	this.structureId = "";
	this.functionId = "";

	this.parentFailureSet = [];
}

FunctionFailure.prototype.appendParentFailure = function(child) {
	for (var i = 0, length = this.parentFailureSet.length; i < length; i++) {
		if(this.parentFailureSet[i].id == child.id){
			return;
		}
	}
    this.parentFailureSet.push(child);
};

/*
	定义功能类
*/
function StructureFunction(name)
{
	this.id = GenerateId();
	this.name = name;
	this.description = "";

	this.structureId = "";

	this.parentFunctionSet = [];
	this.FailureSet = [];
}

StructureFunction.prototype.appendParentFunction = function(child) {
	for (var i = 0, length = this.parentFunctionSet.length; i < length; i++) {
		if(this.parentFunctionSet[i].id == child.id){
			return;
		}
	}
    this.parentFunctionSet.push(child);
};

StructureFunction.prototype.appendFailure = function(child) {
	for (var i = 0, length = this.FailureSet.length; i < length; i++) {
		if(this.FailureSet[i].id == child.id){
			return;
		}
	}
	child.structureId = this.structureId;
	child.functionId = this.id;
    this.FailureSet.push(child);
};

/*
	定义结构树形点类
*/
function StructureNode(name)
{
	this.id = GenerateId();
	this.name = name;
	this.description= "";

	this.uri = "";
	this.html = "";

	this.shape = "";
	this.x = 0;
	this.y = 0;

	this.parent = null;
	this.children = [];

	this.FunctionSet = [];
}

StructureNode.prototype.appendFunction = function(child) {
	for (var i = 0, length = this.FunctionSet.length; i < length; i++) {
		if(this.FunctionSet[i].id == child.id){
			return;
		}
	}

	for (var i = 0, length = child.FailureSet.length; i < length; i++) {
		child.FailureSet[i].structureId = this.id;
	}

	child.structureId = this.id;
    this.FunctionSet.push(child);
};

StructureNode.prototype.render = function(callBack) {
    callBack(this);
};

StructureNode.prototype.appendChild = function(child) {
	child.parent = this;
	for (var i = 0, length = this.children.length; i < length; i++) {
		if(this.children[i].id == child.id){
			return;
		}
	}
    this.children.push(child);
};

StructureNode.prototype.findFirstChild = function(callBack) {
    for (var i = 0, length = this.children.length; i < length; i++) {
    	if(callBack(this.children[i]) == true)
    	{
    		return this.children[i];
    	}
    }

    return null;
};

StructureNode.prototype.findFirstChildren = function(callBack) {
// depth-first search
    (function recurse(currentNode) {
        for (var i = 0, length = currentNode.children.length; i < length; i++) {
            if(callBack(currentNode.children[i]) == true){
	    		return currentNode.children[i];
	    	}

            recurse(currentNode.children[i]);
        }
    })(this);

    return null;
};

StructureNode.prototype.removeChildById = function(id) {
    for (var i = 0, length = this.children.length; i < length; i++) {
    	if(this.children[i].id == id)
    	{
    		this.children[i].parent = null;
    		this.children.splice(i, 1);
    	}
    }
};

StructureNode.prototype.allAboveNode = function() {
	var ndArray = [];
    var parent = this.parent;
    while(parent != null)
    {
    	var grandParent = parent.parent;
    	if(grandParent != null)
    	{
    		for (var i = 0, length = grandParent.children.length; i < length; i++) {
    			ndArray.push(grandParent.children[i]);
    		}
    	}
    	else
    	{
    		ndArray.push(parent);
    	}

    	parent = grandParent;
    }

    return ndArray;
};

/*
	定义结构画布类
*/
function StructurePane(projectName)
{
	this.id = GenerateId();
	this.projectName = projectName;
	this.description = "";

	this.structureTreeRoot = null;
	this.structureNodes = [];
}

StructurePane.prototype.SetStructureTreeRoot = function(node)
{
	node.parent = null;
	this.structureTreeRoot = node;
}

StructurePane.prototype.addStructureNode = function(node)
{
	for (var i = 0, length = this.structureNodes.length; i < length; i++) {
		if(this.structureNodes[i].id == node.id){
			return;
		}
	}

	this.structureNodes.push(node);
}

StructurePane.prototype.findStructureNodeById = function(id)
{
	for (var i = 0, length = this.structureNodes.length; i < length; i++) {
		if(this.structureNodes[i].id == id){
			return this.structureNodes[i];
		}
	}

	return null;
}

StructurePane.prototype.changeStructureNodeParent = function(id, oldParentId, newParentId)
{
	var node = this.findStructureNodeById(id);
	if(node == null)
	{
		return;
	}

	if(oldParentId != "")
	{
		var oldParentNode = this.findStructureNodeById(oldParentId);
		oldParentNode.removeChildById(id);
	}

	var parentNode = this.findStructureNodeById(newParentId);
	if(parentNode != null)
	{
		parentNode.appendChild(node);
	}
}

StructurePane.prototype.deleteStructureNodeById = function(id)
{
	for (var i = 0, length = this.structureNodes.length; i < length; i++) {
		if(this.structureNodes[i].id == id){
			var parentNode = this.structureNodes[i].parent;
			if(parentNode != null)
			{
				parentNode.removeChildById(id)
			}

			for (var j = 0, nlength = this.structureNodes[i].children.length; j < nlength; j++) {
				this.structureNodes[i].children[j].parent = null;
			}

			this.structureNodes.splice(i, 1);
		}
	}
}

StructurePane.prototype.ToJSON = function()
{
	return FMEAObjectToJson(this);
}

StructurePane.prototype.SaveToLocalStorage = function()
{
	localStorage.setItem(this.id, this.ToJSON());
}

function ConvertJsonToStructurePane(jsonString)
{
	var jsonObject = JSON.parse(jsonString);

	var sp = JsonCloneNonObject(jsonObject, StructurePane);

	jsonObject.structureNodes.forEach(function(item, index, array) {
		var node = JsonCloneNonObject(item, StructureNode);
		if(item.hasOwnProperty("parent"))
		{
			node.parent = item.parent; 
		}

	    item.FunctionSet.forEach(function(fsitem, fsindex, fsarray) {
	    	var sf = JsonCloneNonObject(fsitem, StructureFunction);
	    	sf.parentFunctionSet = fsitem.parentFunctionSet;

	    	fsitem.FailureSet.forEach(function(ffitem, ffindex, ffarray) {
	    		var fs = JsonCloneNonObject(ffitem, FunctionFailure);
	    		fs.parentFailureSet = ffitem.parentFailureSet;

	    		sf.FailureSet.push(fs);
	    	});

	    	node.FunctionSet.push(sf);
	    });

	    sp.structureNodes.push(node);
	});

	// build tree mode
	sp.structureTreeRoot = sp.findStructureNodeById(jsonObject.structureTreeRoot.id);
	for (var i = 0, length = sp.structureNodes.length; i < length; i++) {
		var node = sp.structureNodes[i];
		// build structure tree mode
		if(node.parent != null)
		{
			node.parent = sp.findStructureNodeById(node.parent);
			node.parent.appendChild(node);
		}

		// build function tree mode
		for (var j = 0, fslength = node.FunctionSet.length; j < fslength; j++) {
			var fs = node.FunctionSet[j];
			var fsParentFunctionSet = [];
			for (var k = 0, pfslength = fs.parentFunctionSet.length; k < pfslength; k++) {
				var jsonFSParentFunction = fs.parentFunctionSet[k];
				var fsParentNode = sp.findStructureNodeById(jsonFSParentFunction.structureId);
				var fsParentFunction = fsParentNode.FunctionSet.find(item => {
					return item.id === jsonFSParentFunction.id;
				});

				fsParentFunctionSet.push(fsParentFunction);
			}

			fs.parentFunctionSet = fsParentFunctionSet;

			// build failure tree mode
			for (var n = 0, faslength = fs.FailureSet.length; n < faslength; n++) {
				var failure = fs.FailureSet[n];

				var ffparentFailureSet = [];
				for (var m = 0, flength = failure.parentFailureSet.length; m < flength; m++) {
					var parentFailure = failure.parentFailureSet[m];
					var ffParentNode = sp.findStructureNodeById(parentFailure.structureId);
					var ffParentFunction = ffParentNode.FunctionSet.find(item => {
						return item.id === parentFailure.functionId;
					});

					var ffParentFailure = ffParentFunction.FailureSet.find(item => {
						return item.id === parentFailure.id;
					});
					ffparentFailureSet.push(ffParentFailure);
				}

				failure.parentFailureSet = ffparentFailureSet;
			}
		}
	} 

	return sp;
}

function JsonCloneNonObject(jsonObject, toType) {
	var to = new toType();
    if (null == jsonObject || "object" != typeof jsonObject) return to;

    for (var attr in jsonObject) {
        if (jsonObject.hasOwnProperty(attr)){
        	if("object" != typeof jsonObject[attr])
        	{
        		to[attr] = jsonObject[attr];
        	}
        }
    }
    return to;
}

function Clone(obj) {
    if (null == obj || "object" != typeof obj) return obj;
    var copy = {};
    for (var attr in obj) {
        if (obj.hasOwnProperty(attr)) copy[attr] = obj[attr];
    }
    return copy;
}

function FMEAObjectToJson(fmeaObject)
{
	var jsonString =  JSON.stringify(fmeaObject, function(key, value) {
		if (value instanceof StructureNode && value !== null) {
			var temp = Clone(value);
			if(value.parent != null)
			{
				temp.parent = value.parent.id;
			}
			temp.children = [];
			return temp;
		}
		return value;
	});

	return jsonString;
}



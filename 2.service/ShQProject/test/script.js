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

	this.dependentFailureSet = [];

	this.sValue = 0;
	this.oValue = 0;
	this.dValue = 0;
	this.lambdaValue = 0;

	this.detectionSet = [];
	this.preCautionSet = [];
}

FunctionFailure.prototype.appendDependentFailure = function(child) {
	for (var i = 0, length = this.dependentFailureSet.length; i < length; i++) {
		if(this.dependentFailureSet[i].id == child.id){
			return;
		}
	}
    this.dependentFailureSet.push(child);
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

	this.dependentFunctionSet = [];
	this.FailureSet = [];
}

StructureFunction.prototype.appendDependentFunction = function(dependentFunction) {
	for (var i = 0, length = this.dependentFunctionSet.length; i < length; i++) {
		if(this.dependentFunctionSet[i].id == dependentFunction.id){
			return;
		}
	}
    this.dependentFunctionSet.push(dependentFunction);
};

StructureFunction.prototype.appendFailure = function(functionFailure) {
	for (var i = 0, length = this.FailureSet.length; i < length; i++) {
		if(this.FailureSet[i].id == functionFailure.id){
			return;
		}
	}
	functionFailure.structureId = this.structureId;
	functionFailure.functionId = this.id;
    this.FailureSet.push(functionFailure);
};

StructureFunction.prototype.removeFailureById = function(failureId) {
	for (var i = 0, length = this.FailureSet.length; i < length; i++) {
		if(this.FailureSet[i].id == failureId){
			this.FailureSet.splice(i, 1);
    		return;
		}
	}
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

StructureNode.prototype.findFunctionById = function(functionId) {
	for (var i = 0, length = this.FunctionSet.length; i < length; i++) {
		if(this.FunctionSet[i].id == functionId){
			return this.FunctionSet[i];
		}
	}

	return null;
};

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

StructureNode.prototype.removeFunctionById = function(functionId) {
	for (var i = 0, length = this.FunctionSet.length; i < length; i++) {
		if(this.FunctionSet[i].id == functionId){
			this.FunctionSet.splice(i, 1);
    		return;
		}
	}
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

StructureNode.prototype.traverse = function(callback) {
    (function recurse(currentNode) {
        for (var i = 0, length = currentNode.children.length; i < length; i++) {
            recurse(currentNode.children[i]);
        }
 
        callback(currentNode);
         
    })(this);
};

StructureNode.prototype.removeChildById = function(id) {
    for (var i = 0, length = this.children.length; i < length; i++) {
    	if(this.children[i].id == id)
    	{
    		this.children[i].parent = null;
    		this.children.splice(i, 1);
    		return;
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

/*
	methods for structure node
*/
StructurePane.prototype.SetStructureTreeRootById = function(structureNodeId)
{
	var node = this.findStructureNodeById(structureNodeId);
	if(node != null)
	{
		node.parent = null;
		this.structureTreeRoot = node;
	}
	else
	{
		throw "Not found the root node in the StructurePane!";
	}
}

StructurePane.prototype.findStructureNodeById = function(structureNodeId)
{
	for (var i = 0, length = this.structureNodes.length; i < length; i++) {
		if(this.structureNodes[i].id == structureNodeId){
			return this.structureNodes[i];
		}
	}

	return null;
}

StructurePane.prototype.addStructureNode = function(structureNode)
{
	CheckObjectValueType(structureNode, StructureNode);

	var ifexisted = this.findStructureNodeById(structureNode.id);
	if(ifexisted == null)
	{
		this.structureNodes.push(structureNode);
	}
}

StructurePane.prototype.changeStructureNodeParent = function(structureNodeId, newParentId)
{
	var node = this.findStructureNodeById(structureNodeId);
	if(node == null)
	{
		return;
	}

	if(node.parent != null)
	{
		node.parent.removeChildById(structureNodeId);
	}

	var parentNode = this.findStructureNodeById(newParentId);
	if(parentNode != null)
	{
		parentNode.appendChild(node);
	}
}

StructurePane.prototype.deleteStructureNodeById = function(structureNodeId)
{
	for (var i = 0, length = this.structureNodes.length; i < length; i++) {
		if(this.structureNodes[i].id == structureNodeId){
			var parentNode = this.structureNodes[i].parent;
			if(parentNode != null)
			{
				parentNode.removeChildById(structureNodeId)
			}

			for (var j = 0, nlength = this.structureNodes[i].children.length; j < nlength; j++) {
				this.structureNodes[i].children[j].parent = null;
			}

			this.structureNodes.splice(i, 1);
		}
	}
}

/*
	methods for functions and failures
*/
StructurePane.prototype.addFunctionToStructureNode = function(structureNodeId, newFunction)
{
	CheckObjectValueType(newFunction, StructureFunction);

	var node = this.findStructureNodeById(structureNodeId);
	if(node != null)
	{
		node.appendFunction(newFunction);
	}
	else
	{
		throw "The Structure " + structureNodeId + " not found";
	}
}

StructurePane.prototype.deleteFunctionInStructureNode = function(structureNodeId, functionId)
{
	var node = this.findStructureNodeById(structureNodeId);
	if(node != null)
	{
		node.removeFunctionById(functionId);
	}
}

StructurePane.prototype.addFailureToFunction = function(structureNodeId, functionId, newFailure)
{
	CheckObjectValueType(newFailure, FunctionFailure);

	var node = this.findStructureNodeById(structureNodeId);
	if(node != null)
	{
		var structureFunction = node.findFunctionById(functionId);
		if(structureFunction != null)
		{
			structureFunction.appendFailure(newFailure);
		}
		else
		{
			throw "The function " + functionId + " not found";
		}
	}
	else
	{
		throw "The Structure " + structureNodeId + " not found";
	}
}

StructurePane.prototype.deleteFailureInFunction = function(structureNodeId, functionId, failureId)
{
	var node = this.findStructureNodeById(structureNodeId);
	if(node != null)
	{
		var structureFunction = node.findFunctionById(functionId);
		if(structureFunction != null)
		{
			structureFunction.removeFailureById(failureId);
		}
	}
}

StructurePane.prototype.toJSONString = function()
{
	return FMEAObjectToJSONString(this);
}

StructurePane.prototype.saveToLocalStorage = function()
{
	localStorage.setItem(this.id, this.toJSONString());
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
	    	sf.dependentFunctionSet = fsitem.dependentFunctionSet;

	    	fsitem.FailureSet.forEach(function(ffitem, ffindex, ffarray) {
	    		var fs = JsonCloneNonObject(ffitem, FunctionFailure);
	    		fs.dependentFailureSet = ffitem.dependentFailureSet;

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
			var fsdependentFunctionSet = [];
			for (var k = 0, pfslength = fs.dependentFunctionSet.length; k < pfslength; k++) {
				var jsonFSParentFunction = fs.dependentFunctionSet[k];
				var fsParentNode = sp.findStructureNodeById(jsonFSParentFunction.structureId);
				if(fsParentNode == null)
				{
					fsdependentFunctionSet.push(JsonCloneNonObject(jsonFSParentFunction, StructureFunction));
				}
				else
				{
					var fsParentFunction = fsParentNode.FunctionSet.find(item => {
						return item.id === jsonFSParentFunction.id;
					});
					if(fsParentFunction == null)
					{
						fsdependentFunctionSet.push(JsonCloneNonObject(jsonFSParentFunction, StructureFunction));
					}
					else
					{
						fsdependentFunctionSet.push(fsParentFunction);
					}
				}
			}

			fs.dependentFunctionSet = fsdependentFunctionSet;

			// build failure tree mode
			for (var n = 0, faslength = fs.FailureSet.length; n < faslength; n++) {
				var failure = fs.FailureSet[n];

				var ffdependentFailureSet = [];
				for (var m = 0, flength = failure.dependentFailureSet.length; m < flength; m++) {
					var parentFailure = failure.dependentFailureSet[m];
					var ffParentNode = sp.findStructureNodeById(parentFailure.structureId);
					if(ffParentNode == null)
					{
						ffdependentFailureSet.push(JsonCloneNonObject(parentFailure, FunctionFailure));
					}
					else
					{
						var ffParentFunction = ffParentNode.FunctionSet.find(item => {
							return item.id === parentFailure.functionId;
						});
						if(ffParentFunction == null)
						{
							ffdependentFailureSet.push(JsonCloneNonObject(parentFailure, FunctionFailure));
						}
						else
						{
							var ffParentFailure = ffParentFunction.FailureSet.find(item => {
								return item.id === parentFailure.id;
							});
							if(ffParentFailure == null)
							{
								ffdependentFailureSet.push(JsonCloneNonObject(parentFailure, FunctionFailure));
							}
							else
							{
								ffdependentFailureSet.push(ffParentFailure);
							}
						}
					}
					
				}

				failure.dependentFailureSet = ffdependentFailureSet;
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

function FMEAObjectToJSONString(fmeaObject)
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

function CheckObjectValueType(objectValue, objectType)
{
	if ((objectValue instanceof objectType) == false)
	{
		throw "Object value type error, please check the input value type! The expected value type is " + objectType;
	}
}



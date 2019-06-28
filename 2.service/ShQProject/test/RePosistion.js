

function RePositionTree(jsonTree, xSpace, ySpace)
{
	var rootNodes = jsonTree.nodes.filter(node=>{return node.shape == "square";});
	if(rootNodes.length == 0)
	{
		return jsonTree;
	}

	var rootBaseX = rootNodes[0].x;
	var rootBaseY = rootNodes[0].y;
	for(var i = 0; i < rootNodes.length; i++)
		{
			var rootNode = rootNodes[i];
			rootNode.x = rootBaseX;
			rootNode.y = rootBaseY;
			var uiNodessInSameLayer = [];
			uiNodessInSameLayer.push(rootNode);

			(function Recurse(parentNodes)
			{
				var uiNodes = [];
				for(var i = 0; i < parentNodes.length; i++)
				{
					var edges = jsonTree.edges.filter(edge=>{return edge.source === parentNodes[i].id;});
					for(var j = 0; j < edges.length; j++)
					{
						var childNode = jsonTree.nodes.find(node => {return node.id == edges[j].target;});
						if(childNode != undefined)
						{
							uiNodes.push(childNode);
						}
					}
				}

				if(uiNodes.length > 0)
				{
					var x = rootBaseX - (uiNodes.length - 1) * xSpace / 2;
					rootBaseY = rootBaseY + ySpace;

					for(var u = 0; u < uiNodes.length; u++)
					{
						uiNodes[u].x = x;
						uiNodes[u].y = rootBaseY;
						x = x + xSpace;
					}

					Recurse(uiNodes);
				}

			})(uiNodessInSameLayer);

			rootBaseY = rootBaseY + ySpace;
		}

	return jsonTree;
}
#coding:utf-8
#!C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Python 3.5
#usage: run.py --C=<project ID>

import sys
import getopt
import numpy 
import pymysql
import copy

global HOST,USER,PASSWD,DB,TASK

#########################################################

def init():
	global HOST,USER,PASSWD,DB
	HOST = "127.0.0.1"
	USER = "root"
	PASSWD = "postman"
	DB = "saic"

#########################################################

def showUsage():
	print("usage: run.py --C=<project ID>")

#########################################################

def getArgv(argv):
	res = {}
	try:
		opts,args = getopt.getopt(argv,"h",["help","C="])
	except getopt.GetoptError:
		showUsage()
		sys.exit(-1)

	for opt,arg in opts:
		if opt == "-h" or opt == "--help":
			showUsage()
			sys.exit(-1)
		elif opt == "--C":
			res["CUT"] = arg
	return res

#########################################################

def getDBConnection():
	global HOST,USER,PASSWD,DB
	conn = pymysql.connect(HOST,USER,PASSWD,DB)
	return conn

#########################################################

def releaseDBConnection(conn):
	conn.close()

#########################################################

def getNodeInfo(conn,pid):
	res = dict()
	sqlStr = "select a.NODE_ID,a.NODE_NAME,c.NODE_TYPE_DESC,a.PARENT_ID,b.GATE_ID,b.GATE_NAME,d.GATE_TYPE_DESC \
              from t_node a \
              LEFT join t_gate b \
              on a.GATE_ID = b.GATE_ID \
              and b.PROJECT_ID = a.PROJECT_ID \
              LEFT JOIN t_node_type_dic c \
              on a.NODE_TYPE = c.NODE_TYPE \
              LEFT JOIN t_gate_type_dic d \
              on b.GATE_TYPE = d.GATE_TYPE \
              where a.PROJECT_ID = %s order by a.NODE_TYPE"
	cur = conn.cursor()
	cur.execute(sqlStr,pid)
	results = cur.fetchall()
	for r in results:
		res[r[0]] = {"NODE_NAME":r[1],"NODE_TYPE_DESC":r[2],"PARENT_ID":r[3],"GATE_ID":r[4],"GATE_NAME":r[5],"GATE_TYPE_DESC":r[6]}
	cur.close()
	return res

#########################################################

def getRootInfo(conn,pid):
	res = ""
	sqlStr = "select NODE_ID from t_node where PROJECT_ID = %s and NODE_TYPE = 1"
	cur = conn.cursor()
	cur.execute(sqlStr,pid)
	res = cur.fetchone()[0]
	cur.close()
	return res

#########################################################

def getCUTSCombine(cuts,childGate,linkNodeID,remove):
	if childGate["GATE_TYPE_DESC"] in ("AND"):
		for n in cuts:
			if linkNodeID in n:
				m = copy.deepcopy(n)
				n.remove(linkNodeID)
				for t in childGate["CUTS"]:
					n.extend(t)
		if not remove:
			cuts.append(m)
	elif childGate["GATE_TYPE_DESC"] in ("OR","XOR"):
		for n in cuts:
			if linkNodeID in n:
				m = copy.deepcopy(n)
				n.remove(linkNodeID)
				for t in childGate["CUTS"]:
					temp = copy.deepcopy(n)
					temp.extend(t)
					cuts.extend([temp])
					del temp
				cuts.remove(n)	
		if not remove:
			cuts.append(m)	

#########################################################

def getBranchs(cuts,nodes):
	branchs = list()
	for n in cuts:
		for m in n:
			if nodes[m]["NODE_TYPE_DESC"] in ("BRANCH"):
				branchs.append(m)
	return branchs

#########################################################

def getChildGates(branch,nodes):
	childGates = dict()
	for n in nodes.keys():
		if nodes[n]["PARENT_ID"] == branch:
			myKey = (nodes[n]["GATE_ID"],nodes[n]["GATE_TYPE_DESC"])
			if myKey not in childGates.keys():
				childGates[myKey] = {"CUTS":list()}
			if nodes[n]["GATE_TYPE_DESC"] in ("AND"):
				if childGates[myKey]["CUTS"]:
					childGates[myKey]["CUTS"][0].append(n)
				else:
					childGates[myKey]["CUTS"].append([n])
			if nodes[n]["GATE_TYPE_DESC"] in ("OR","XOR"):
				childGates[myKey]["CUTS"].append([n])
	return childGates

#########################################################

def checkLeafs(cuts,nodes):
	branchs = list()
	branchs = getBranchs(cuts,nodes)
	for r in branchs:
		childGates = getChildGates(r,nodes)
		n = 0
		remove = False
		for k in childGates.keys():
			n = n + 1
			if n == len(childGates):
				remove = True 
			getCUTSCombine(cuts,{"GATE_TYPE_DESC":k[1],"CUTS":childGates[k]["CUTS"]},r,remove)
	return False

#########################################################

def getCUTS(root,nodes):
	res = dict()
	cuts = list()
	for k in nodes.keys():
		if nodes[k]["PARENT_ID"] == root:
			if nodes[k]["GATE_ID"] not in res.keys():
				res[nodes[k]["GATE_ID"]] = {"GATE_NAME":nodes[k]["GATE_NAME"],"GATE_TYPE_DESC":nodes[k]["GATE_TYPE_DESC"],"CUTS":list()}
			if nodes[k]["GATE_TYPE_DESC"] in ("AND"):
				if (res[nodes[k]["GATE_ID"]]["CUTS"]):
					res[nodes[k]["GATE_ID"]]["CUTS"][0].append(k)
				else:
					res[nodes[k]["GATE_ID"]]["CUTS"].append([k])
			elif nodes[k]["GATE_TYPE_DESC"] in ("OR","XOR"):
				res[nodes[k]["GATE_ID"]]["CUTS"].append([k])
	for k in res.keys():
		cuts.extend(res[k]["CUTS"])
	while checkLeafs(cuts,nodes):		
		None
	return cuts

#########################################################

def computeForCUTS(pid):
	print("the project ID is:"+pid)
	conn = getDBConnection()
	nodes = getNodeInfo(conn,pid)
	root = getRootInfo(conn,pid)
	cuts = getCUTS(root,nodes)
	releaseDBConnection(conn)
	print("the cuts is:"+str(cuts))
	return cuts

#########################################################

if __name__=="__main__":
	print("kick off the program")
	task = {}
	init()
	task = getArgv(sys.argv[1:])
	for t in task.keys():
		if t == "CUT":
			cuts = computeForCUTS(task[t])
			
	print("the program is completed")

	
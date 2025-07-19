// Copyright 2012 by Livingston Technologies
// All rights reserved.

function A(Owner, Armies, NewArmies, Target, TargetAmount)
{
	this.Owner = Owner;
	this.Armies = Armies;
	this.NewArmies = NewArmies;
	this.Target = Target;
	this.TargetAmount = TargetAmount;
}

function S(Link_1, Link_2, Link_3, Link_4, Link_5, Link_6, CenterX, CenterY)
{
	this.Link_1 = Link_1;
	this.Link_2 = Link_2;
	this.Link_3 = Link_3;
	this.Link_4 = Link_4;
	this.Link_5 = Link_5;
	this.Link_6 = Link_6;
	this.CenterX = CenterX;
	this.CenterY = CenterY;
}

function Done() {
    if (window.webkitNotifications && window.webkitNotifications.checkPermission() != 0)
        window.webkitNotifications.requestPermission(function () { $.post("Done"); });
    else
        $.post("Done");

    document.getElementById("EndTurnLink").style.display = "none";
}

function ForceTurn() {
    $.post("ForceTurn");
}

function OnClick(Area)
{
	if (ActiveArea == 0)
	{
		if (AD[Area].Owner == CurrentPlayer)
			ShowControl(Area);
	}
	else
	    SelectTarget(Area);
}

function ShowControl(Area)
{
	ClearMode();
	ActiveArea = Area;
	document.getElementById("ActiveArea").value = Area;

	// update links
	for (var LCount = 1; LCount <= NumAreas; LCount++)
	{
		if (SD[ActiveArea].Link_1 != LCount && SD[ActiveArea].Link_2 != LCount &&
			SD[ActiveArea].Link_3 != LCount && SD[ActiveArea].Link_4 != LCount &&
			SD[ActiveArea].Link_5 != LCount && SD[ActiveArea].Link_6 != LCount &&
			ActiveArea != LCount )
		{
		    currentArea = document.getElementById("Area" + LCount);
		    if (currentArea)
		        currentArea.style.visibility = "hidden";
		}
	}

	document.getElementById("PlayerReadout").style.visibility = "hidden";

	if (AD[Area].Owner == CurrentPlayer)
	{
		if (AD[Area].Target != 0)
		{
			document.getElementById("ActionMessage").innerHTML += "<br>(Currently ";
			if (AD[AD[Area].Target].Owner == CurrentPlayer)
				document.getElementById("ActionMessage").innerHTML += "Transfering " + AD[Area].TargetAmount + " to ";
			else
				document.getElementById("ActionMessage").innerHTML += "Attacking ";
			document.getElementById("ActionMessage").innerHTML += document.getElementById("Area" + AD[Area].Target).title;
			if (AD[AD[Area].Target].Owner != CurrentPlayer)
				document.getElementById("ActionMessage").innerHTML += " with " + AD[Area].TargetAmount;
			document.getElementById("ActionMessage").innerHTML += ") ";
		}
		if (AD[Area].NewArmies != 0)
		    document.getElementById("ActionMessage").innerHTML += "<br />[ <a href=\"javascript:UnassignArmies();\">Unassign</a> ]";
        else if (NewArmies == 0)
            document.getElementById("ActionMessage").innerHTML += "<br />";
    }

    document.getElementById("EntryForm").style.visibility = "visible";

    if (NewArmies > 0)
        document.getElementById("AmountInput").focus(); 
}

function ShowAreaArmies(Area)
{
	var armyCount = AD[Area].Armies;
	if (armyCount != -1)
	{
//	    $("#Area" + Area).offset({ top: SD[Area].CenterY - 5, left: SD[Area].CenterX - (armyCount.toString().length * 5) })
	    $("#Area" + Area).css({ top: SD[Area].CenterY - 5, left: SD[Area].CenterX - (armyCount.toString().length * 5) });

		var armyImages = "";
		var tempDigit = 0;
		if (armyCount >= 100)
		{
			tempDigit = Math.floor(armyCount / 100);
			armyImages += "<img src=/images/" + tempDigit + "x.gif>";
		}
		if (armyCount >= 10)
		{
			tempDigit = Math.floor((armyCount % 100) / 10);
			armyImages += "<img src=/images/" + tempDigit + "x.gif>";
		}
		tempDigit = armyCount % 10;
		armyImages += "<img src=/images/" + tempDigit + ".gif>";

		var span = document.getElementById("Area" + Area);
		span.innerHTML = "<a href=\"javascript:OnClick(" + Area + ");\" class=armyNav>" + armyImages + "</a>";
	}
}

function ShowAllAreaArmies()
{
	for (var LCount = 1; LCount <= NumAreas; LCount++)
		ShowAreaArmies(LCount);
}

function ShowNewArmyCount()
{
	document.getElementById("NewArmyCount").innerHTML = NewArmies;
}

function SelectTarget(Area)
{
	if (Area == ActiveArea)
		return;
	document.getElementById("AssignFlag").value = 0;
	document.getElementById("TransferFlag").value = 0;
	document.getElementById("AttackFlag").value = 0;
	if (AD[Area].Owner == CurrentPlayer)
	{
		//Transfer
		document.getElementById("ActionMessage").innerHTML = "Transfer how many armies to " + document.getElementById("Area" + Area).title + "?";
		document.getElementById("ActionSubmit").value = "Transfer";
		document.getElementById("TransferFlag").value = Area;
	}
	else
	{
		//Attack
		document.getElementById("ActionMessage").innerHTML = "Attack " + document.getElementById("Area" + Area).title + " with how many armies?";
		document.getElementById("ActionSubmit").value = "Attack";
		document.getElementById("AttackFlag").value = Area;
	}
	TargetArea = Area;
	document.getElementById("AmountInput").value = AD[ActiveArea].Armies - 1;
	document.getElementById("AmountField").style.display = "inline";
	document.getElementById("AmountInput").focus(); 
}

function ClearMode()
{
	TargetArea = 0;
	document.getElementById("AssignFlag").value = 0;
	document.getElementById("TransferFlag").value = 0;
	document.getElementById("AttackFlag").value = 0;
	if(NewArmies == 0)
	{
		document.getElementById("ActionMessage").innerHTML = "Select an area to attack or transfer to.";
		document.getElementById("AmountField").style.display = "none";
    }
	else
	{
		document.getElementById("AssignFlag").value = 1;
		document.getElementById("ActionMessage").innerHTML = "Assign new armies or select a target area.";
		document.getElementById("AmountInput").value = NewArmies;
		document.getElementById("AmountField").style.display = "inline";
		document.getElementById("ActionSubmit").value = "Assign";
	}
}

function UnassignArmies()
{
    SubmitActiveArea = ActiveArea;

    $.post("Unassign",
            { areaId: ActiveArea, amount: document.getElementById("AmountInput").value },
            function (result) {
                result = parseInt(result);
                NewArmies += result;
                AD[SubmitActiveArea].Armies -= result;
                AD[SubmitActiveArea].NewArmies -= result;
                ShowAreaArmies(SubmitActiveArea);
                ShowNewArmyCount();
            });

	CancelAction();
}

function SubmitAction()
{
    SubmitActiveArea = ActiveArea;
    SubmitTargetArea = TargetArea;

    if (TargetArea == 0)
    {
	    $.post("Assign",
            { areaId: ActiveArea, amount: document.getElementById("AmountInput").value },
            function (result) {
                result = parseInt(result);
                NewArmies -= result;
                ShowNewArmyCount();
                AD[SubmitActiveArea].Armies += result;
                AD[SubmitActiveArea].NewArmies += result;
                ShowAreaArmies(SubmitActiveArea);
            });
	}
	else
	{
	    if (AD[TargetArea].Owner == CurrentPlayer) {
	        $.post("Transfer",
                { areaId: ActiveArea, targetAreaId: TargetArea, amount: document.getElementById("AmountInput").value },
                function (result) {
                    result = parseInt(result);
                    AD[SubmitActiveArea].Target = SubmitTargetArea;
                    AD[SubmitActiveArea].TargetAmount = result;
                });
	    }
	    else {
	        $.post("Attack",
                { areaId: ActiveArea, targetAreaId: TargetArea, amount: document.getElementById("AmountInput").value },
                function (result) {
                    result = parseInt(result);
                    AD[SubmitActiveArea].Target = SubmitTargetArea;
                    AD[SubmitActiveArea].TargetAmount = result;
                });
	    }
    }

    CancelAction();

    return false;
}

function CancelAction()
{
	ActiveArea = 0;
	document.getElementById("ActiveArea").value = 0;
	ClearMode();
	document.getElementById("AmountField").style.display = "none";
	document.getElementById("EntryForm").style.visibility = "hidden";
	document.getElementById("PlayerReadout").style.visibility = "visible";

	// reset links
	for (var LCount = 1; LCount <= NumAreas; LCount++)
	{
	    currentArea = document.getElementById("Area" + LCount);
	    if (currentArea)
		    currentArea.style.visibility = "visible";
	}
}

var SubmitActiveArea = 0;
var ActiveArea = 0;
var TargetArea = 0;

function TimeTillKill(TimeLeft, GameID)
{
	var tmpDate = new Date;
	var curTime = (TimeLeft * 1000) - (tmpDate.getTime() - startTime);
	var tmpStr = '';
	if (curTime > 0)
	{
		if (curTime < 60000)
			tmpStr = '<span class="error">';
		tmpStr += Math.floor(curTime/3600000) + ':';
		if (Math.floor((curTime % 3600000)/60000) < 10)
			tmpStr += '0';
		tmpStr += Math.floor((curTime % 3600000)/60000) + ':';
		if (Math.floor((curTime/1000) % 60) < 10)
			tmpStr += '0';
		tmpStr += Math.floor((curTime/1000) % 60);
		setTimeout('TimeTillKill(' +  TimeLeft + ', ' + GameID + ')', 1000);
	} else {
		tmpStr = '<a href=\'javascript:if (confirm("Are you sure you wish to force the turn?")) { ForceTurn(); }\'><span class="error" >Force Turn</span></a>';
	}
	document.getElementById("gameTimer").innerHTML=tmpStr;
}

var tmpDate = new Date;
var startTime = tmpDate.getTime();
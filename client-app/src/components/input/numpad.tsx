import React from "react";

import styles from "./numpad.module.scss";
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord';
import CheckIcon from '@mui/icons-material/Check';
import ClearIcon from '@mui/icons-material/Clear';

//Link to grid layout grid.layoutit.com?id=GINfCeW

/*
Handles the input cases from each of the buttons on the number pad.
Non-number buttons are represented using negatives.
-4 = negative
-3 = clear
-2 = submit
-1 = decimal point
*/
function handleNumpadButtonClick(num: number){
    //console.log("Numpad " + num + " was pressed!")
    var display = document.getElementsByClassName("numpad_display_text")[0];
    var lastChar = display.innerHTML.charAt(display.innerHTML.length -1);
    switch(num){      
        case -1:
            if(!(lastChar===".")){
                display.innerHTML=display.innerHTML+".";
            }         
            break;
        case -2:
            display.innerHTML="&gt;";
            //Handle submit
            break;
        case -3:
            display.innerHTML="&gt;";
            break;
        case -4:
            //console.log(display.innerHTML.charAt(display.innerHTML.length -1));           
            //console.log(lastChar + "|" + !(lastChar === '.'));
            if(isNaN(parseInt(lastChar)) && !(lastChar === '.')){ //Only allow - if the last number was not another number or a decimal point. Example of what this prevents: "324-3" and "3.4-4"
                display.innerHTML=display.innerHTML+"-";
            }
            break;
        default: 
            display.innerHTML=display.innerHTML+num;
            break;
    }
}


//Do we choose to implement a backspace button OR shall we let that add to the pressure? hmmmmm.
const NumPad: React.FC = (props) => {
    return (
        <div>
            <span className={styles.numpad_display}><h3 className="numpad_display_text">&gt;</h3></span>
            <div className={styles.numpad_container}>
            <button onClick={() =>handleNumpadButtonClick(7)} className={[styles.num7, styles.numpad_button].join(" ")}>7</button>
            <button onClick={() =>handleNumpadButtonClick(8)} className={[styles.num8, styles.numpad_button].join(" ")}>8</button>
            <button onClick={() =>handleNumpadButtonClick(9)} className={[styles.num9, styles.numpad_button].join(" ")}>9</button>
            <button onClick={() =>handleNumpadButtonClick(4)} className={[styles.num4, styles.numpad_button].join(" ")}>4</button>
            <button onClick={() =>handleNumpadButtonClick(5)} className={[styles.num5, styles.numpad_button].join(" ")}>5</button>
            <button onClick={() =>handleNumpadButtonClick(6)} className={[styles.num6, styles.numpad_button].join(" ")}>6</button>
            <button onClick={() =>handleNumpadButtonClick(1)} className={[styles.num1, styles.numpad_button].join(" ")}>1</button>
            <button onClick={() =>handleNumpadButtonClick(2)} className={[styles.num2, styles.numpad_button].join(" ")}>2</button>
            <button onClick={() =>handleNumpadButtonClick(3)} className={[styles.num3, styles.numpad_button].join(" ")}>3</button>
            <button onClick={() =>handleNumpadButtonClick(0)} className={[styles.num0, styles.numpad_button].join(" ")}>0</button>
            <button onClick={() =>handleNumpadButtonClick(-1)} className={[styles.numdot, styles.numpad_button].join(" ")}><FiberManualRecordIcon sx={{ fontSize: "75%" }} /></button>
            <button onClick={() =>handleNumpadButtonClick(-2)} className={[styles.numenter, styles.numpad_button].join(" ")}><span><CheckIcon sx={{ fontSize: "100%"}} /></span></button>
            <button onClick={() =>handleNumpadButtonClick(-3)} className={[styles.numclear, styles.numpad_button].join(" ")}><span><ClearIcon sx={{ fontSize: "150%", lineHeight: "100%"}} /></span></button>
            <button onClick={() =>handleNumpadButtonClick(-4)} className={[styles.numneg, styles.numpad_button].join(" ")}>-</button>
        </div>
        </div>
    );
};

export default NumPad;
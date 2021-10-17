import Question from "components/Question";
import React, { useState } from "react";
import ProgressBar from "components/ProgressBar";
import useForm from "hooks/useForm";

interface ProgressFormState {
    answer: string,
};
const Home: React.FC = () => {
    const initialState: ProgressFormState = {
        answer: ""
    };
 
    const [value, setValue] = useState(0);

    // a submit function that will execute upon form submission
    async function ProgressCallback() {
        // send "values" to database
        // alert('You have submitted the form.')
        if (formState.answer === "1"){
            setValue(value + 1)
        }
        if (value >= 5){
            setValue(0)
        }
    }


    // getting the event handlers from our custom hook
    const [formState, onFormChange, onFormSubmit] = useForm(
        ProgressCallback,
        initialState
    );

    return (
        <div>
            <h3>Home</h3>
            <ProgressBar value ={value}/>
            <Question id={4} />
            <form onSubmit={onFormSubmit}>
                <input name="answer" value={formState.answer} placeholder="answer" type="text" onChange={onFormChange} />
                <input type="submit" value="Submit" />
            </form>
        </div>
    );
};

export default Home;
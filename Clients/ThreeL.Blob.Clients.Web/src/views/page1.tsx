import { useSelector, useDispatch } from "react-redux"
const Page1: React.FC = () => {
    const dispatch = useDispatch();
    const num = useSelector((state: RootState) => state.numReducer.num);
    const ChangNum = () => {
        dispatch({ type: "addNum1", val: 1 })
    }
    const ChangNum1 = () => {
        dispatch({ type: "addNum2", val: 5 })
    }
    return (
        <div>
            头头头2222
            <p>{num}</p>
            <button onClick={ChangNum}>惦记我</button>
            <button onClick={ChangNum1}>惦记我111</button>
        </div>)
}
export default Page1
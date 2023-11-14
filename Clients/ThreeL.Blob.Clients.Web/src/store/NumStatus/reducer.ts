import NumState from './index';

let reducer = (state = {...NumState.state}, action: { type: string, val: number }) => {
    let newState = JSON.parse(JSON.stringify(state))
    for (let key in NumState.actionNames) {
        if(key === action.type) {
            NumState.actions[key](newState, action)
        }
    }

    return newState;
}

export default reducer
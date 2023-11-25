import LoginUserState from './index';

let reducer = (state = {...LoginUserState.state}, action: { type: string, val: { id: number, userName: string, role: string, accessToken: string } }) => {
    let newState = JSON.parse(JSON.stringify(state))
    for (let key in LoginUserState.actionNames) {
        if(key === action.type) {
            LoginUserState.actions[key](newState, action)
        }
    }

    return newState;
}

export default reducer
import UpdateCreateNewUserState from './index';

let reducer = (state = { ...UpdateCreateNewUserState.state }, action: { type: string, val: string }) => {
    let newState = JSON.parse(JSON.stringify(state))
    for (let key in UpdateCreateNewUserState.actionNames) {
        if (key === action.type) {
            UpdateCreateNewUserState.actions[key](newState, action)
        }
    }

    return newState;
}

export default reducer
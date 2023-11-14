const defaultState = {
    num: 20
}
let reducer = (state = defaultState, action: { type: string, val: number }) => {
    switch (action.type) {
        case 'ADD':
            state.num += action.val
    }

    return JSON.parse(JSON.stringify(state))
}

export default reducer
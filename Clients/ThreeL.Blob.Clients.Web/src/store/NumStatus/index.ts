interface SimpleKeyValueObject {
    [key: string]: any
}
interface CommonStore {
    state: object;
    actions: SimpleKeyValueObject;
    actionNames: SimpleKeyValueObject;
}

const store: CommonStore = {
    state: {
        num: 0
    },
    actions: {
        addNum1(newState: { num: number }, action: { val: number }) { // action:{type:string} is optional
            newState.num += 1
        },
        addNum2(newState: { num: number }, action: { val: number }) { // action:{type:string} is optional
            newState.num += action.val
        }
    },
    actionNames: {}
}

for (let key in store.actions) {
    store.actionNames[key] = key
}

export default store

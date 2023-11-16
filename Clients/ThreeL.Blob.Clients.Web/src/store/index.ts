import { legacy_createStore, combineReducers } from 'redux'
import rootReducer from './reducer'
import numReducer from './NumStatus/reducer.ts'

const reducers = combineReducers({ numReducer });
const store = legacy_createStore(reducers)

export default store